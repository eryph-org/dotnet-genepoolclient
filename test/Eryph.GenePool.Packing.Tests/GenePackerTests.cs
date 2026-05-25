using System.Text.Json;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing.Tests;

public sealed class GenePackerTests : IDisposable
{
    private readonly string _workDir;

    public GenePackerTests()
    {
        _workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_workDir);
    }

    [Fact]
    public async Task CreateGene_CompressionNone_WritesPlainFormatEvenForLargeFile()
    {
        var sourcePath = Path.Combine(_workDir, "volume.qcow2");
        await File.WriteAllBytesAsync(sourcePath, new byte[GeneModelDefaults.MinCompressionBytes * 2]);

        var packable = new PackableFile(sourcePath, "volume.qcow2",
            GeneType.Volume, Architectures.KvmAmd64,
            "volume", GeneCompression.None, null);

        var hashRef = await GenePacker.CreateGene(packable, _workDir);

        var manifest = ReadManifest(hashRef);
        manifest.Format.Should().Be("plain");
    }

    [Fact]
    public async Task CreateGene_CompressionExtreme_WritesXzFormat()
    {
        var sourcePath = Path.Combine(_workDir, "volume.vhdx");
        await File.WriteAllBytesAsync(sourcePath, new byte[GeneModelDefaults.MinCompressionBytes * 2]);

        var packable = new PackableFile(sourcePath, "volume.vhdx",
            GeneType.Volume, Architectures.HyperVAmd64,
            "volume", GeneCompression.Extreme, null);

        var hashRef = await GenePacker.CreateGene(packable, _workDir);

        var manifest = ReadManifest(hashRef);
        manifest.Format.Should().Be("xz");
    }

    [Fact]
    public async Task CreateGene_CompressionDefault_LargeFile_WritesGzFormat()
    {
        var sourcePath = Path.Combine(_workDir, "catlet.json");
        await File.WriteAllBytesAsync(sourcePath, new byte[GeneModelDefaults.MinCompressionBytes * 2]);

        var packable = new PackableFile(sourcePath, "catlet.json",
            GeneType.Catlet, Architectures.Any,
            "catlet", GeneCompression.Default, null);

        var hashRef = await GenePacker.CreateGene(packable, _workDir);

        var manifest = ReadManifest(hashRef);
        manifest.Format.Should().Be("gz");
    }

    private GeneManifestData ReadManifest(string hashRef)
    {
        var hash = hashRef.Split(':')[1];
        var manifestPath = Path.Combine(_workDir, hash, "gene.json");
        var json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<GeneManifestData>(json, GeneModelDefaults.SerializerOptions)!;
    }

    public void Dispose()
    {
        if (Directory.Exists(_workDir))
            Directory.Delete(_workDir, true);
    }
}
