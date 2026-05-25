using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing.Tests;

public sealed class VMExportTests : IDisposable
{
    private readonly DirectoryInfo _exportDir;

    public VMExportTests()
    {
        _exportDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        _exportDir.Create();
    }

    [Fact]
    public void ExportToPackable_VhdxFile_ReturnsHyperVVolumeWithExtremeCompression()
    {
        var vhdxPath = Path.Combine(_exportDir.FullName, "sda.vhdx");
        File.WriteAllBytes(vhdxPath, [1, 2, 3]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        files.Should().ContainSingle(f => f.FileName == "sda.vhdx")
            .Which.Should().BeEquivalentTo(new
            {
                GeneType = GeneType.Volume,
                Architecture = Architectures.HyperVAmd64,
                GeneName = "sda",
                Compression = GeneCompression.Extreme,
            });
    }

    [Fact]
    public void ExportToPackable_Qcow2File_ReturnsKvmVolumeWithoutCompression()
    {
        var qcow2Path = Path.Combine(_exportDir.FullName, "sda.qcow2");
        File.WriteAllBytes(qcow2Path, [1, 2, 3]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        files.Should().ContainSingle(f => f.FileName == "sda.qcow2")
            .Which.Should().BeEquivalentTo(new
            {
                GeneType = GeneType.Volume,
                Architecture = Architectures.KvmAmd64,
                GeneName = "sda",
                Compression = GeneCompression.None,
            });
    }

    [Fact]
    public void ExportToPackable_MixedFiles_ReturnsBothArchitectures()
    {
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sda.vhdx"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sdb.qcow2"), [4, 5, 6]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        var fileList = files.ToList();
        fileList.Should().HaveCount(2);
        fileList.Should().Contain(f => f.Architecture == Architectures.HyperVAmd64 && f.Compression == GeneCompression.Extreme);
        fileList.Should().Contain(f => f.Architecture == Architectures.KvmAmd64 && f.Compression == GeneCompression.None);
    }

    [Fact]
    public void ExportToPackable_VolumesScanned_AppendsDrivesByGeneName()
    {
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sda.vhdx"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sda.qcow2"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sdb.qcow2"), [4, 5, 6]);

        var (config, _) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        config!.Drives.Should().NotBeNull();
        config.Drives!.Select(d => d.Name).Should().BeEquivalentTo("sda", "sdb");
    }

    [Fact]
    public void ExportToPackable_CatletYamlPresent_UsesItOverVmJson()
    {
        File.WriteAllText(Path.Combine(_exportDir.FullName, "catlet.yaml"),
            "cpu:\n  count: 4\nmemory:\n  startup: 4096\n");
        File.WriteAllText(Path.Combine(_exportDir.FullName, "vm.json"),
            "{\"vm\":{\"ProcessorCount\":1,\"MemoryStartup\":1073741824}}");

        var (config, _) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        config!.Cpu!.Count.Should().Be(4);
        config.Memory!.Startup.Should().Be(4096);
    }

    [Fact]
    public void ExportToPackable_VhdFile_TreatedLikeVhdx()
    {
        var vhdPath = Path.Combine(_exportDir.FullName, "sda.vhd");
        File.WriteAllBytes(vhdPath, [1, 2, 3]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        files.Should().ContainSingle(f => f.FileName == "sda.vhd")
            .Which.Should().BeEquivalentTo(new
            {
                Architecture = Architectures.HyperVAmd64,
                Compression = GeneCompression.Extreme,
            });
    }

    [Fact]
    public void ExportToPackable_FilesInHypervisorProcessorFolders_UseFolderArchitecture()
    {
        var hyperv = Directory.CreateDirectory(Path.Combine(_exportDir.FullName, "hyperv", "amd64"));
        var kvm = Directory.CreateDirectory(Path.Combine(_exportDir.FullName, "kvm", "amd64"));
        var azure = Directory.CreateDirectory(Path.Combine(_exportDir.FullName, "azure", "amd64"));
        File.WriteAllBytes(Path.Combine(hyperv.FullName, "sda.vhdx"), [1]);
        File.WriteAllBytes(Path.Combine(kvm.FullName, "sda.qcow2"), [1]);
        File.WriteAllBytes(Path.Combine(azure.FullName, "sda.vhd"), [1]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        var fileList = files.ToList();
        fileList.Should().Contain(f => f.FileName == "sda.vhdx" && f.Architecture == Architectures.HyperVAmd64);
        fileList.Should().Contain(f => f.FileName == "sda.qcow2" && f.Architecture == Architectures.KvmAmd64);
        fileList.Should().Contain(f => f.FileName == "sda.vhd" && f.Architecture == Architectures.AzureAmd64
                                       && f.Compression == GeneCompression.Extreme);
    }

    [Fact]
    public void ExportToPackable_FileInHypervisorFolderWithoutProcessor_UsesAnyArchitecture()
    {
        var kvmDir = Directory.CreateDirectory(Path.Combine(_exportDir.FullName, "kvm"));
        File.WriteAllBytes(Path.Combine(kvmDir.FullName, "sda.qcow2"), [1]);

        var (_, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        files.Should().ContainSingle()
            .Which.Architecture.Should().Be(Architectures.KvmAny);
    }

    [Fact]
    public void ExportToPackable_CatletYamlPresent_DrivesNotAutoMergedFromScan()
    {
        File.WriteAllText(Path.Combine(_exportDir.FullName, "catlet.yaml"),
            "drives:\n- name: sda\n  size: 50\n");
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sda.qcow2"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(_exportDir.FullName, "sdb.qcow2"), [4, 5, 6]);

        var (config, files) = VMExport.ExportToPackable(_exportDir, CancellationToken.None);

        // catlet.yaml is authoritative for drives: sdb on disk must NOT be added
        config!.Drives.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Name = "sda", Size = 50 });

        // ...but the volume files themselves are still packed (sdb becomes an orphan gene)
        files.Should().HaveCount(2);
    }

    public void Dispose()
    {
        if (_exportDir.Exists)
            _exportDir.Delete(true);
    }
}
