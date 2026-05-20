using System.Text.Json;
using System.Text.Json.Nodes;
using Eryph.ConfigModel.Catlets;
using Eryph.ConfigModel.Yaml;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing;

public static class VMExport
{
    private static readonly Dictionary<string, (string DefaultArchitecture, GeneCompression Compression)>
        VolumeExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            [".vhdx"] = (Architectures.HyperVAmd64, GeneCompression.Extreme),
            [".vhd"]  = (Architectures.HyperVAmd64, GeneCompression.Extreme),
            [".qcow2"] = (Architectures.KvmAmd64, GeneCompression.None),
        };

    public static (CatletConfig? Config, IEnumerable<PackableFile> Files) ExportToPackable(DirectoryInfo vmExport,
        CancellationToken token)
    {
        var files = new List<PackableFile>();
        var catletFromYaml = ReadCatletConfig(vmExport);
        var vmPlan = catletFromYaml
                     ?? ConvertVmDataToConfig(vmExport)
                     ?? new CatletConfig();

        foreach (var volumeFile in vmExport.GetFiles("*", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();

            if (!VolumeExtensions.TryGetValue(volumeFile.Extension, out var spec))
                continue;

            var architecture = DetectArchitectureFromPath(volumeFile, vmExport) ?? spec.DefaultArchitecture;

            files.Add(new PackableFile(volumeFile.FullName, volumeFile.Name,
                GeneType.Volume,
                architecture,
                Path.GetFileNameWithoutExtension(volumeFile.Name), spec.Compression, null));
        }

        // When the user supplies catlet.yaml, it is the source of truth for drives.
        // Only the legacy vm.json / no-config path gets drives auto-derived from the volume scan.
        if (catletFromYaml is null)
            MergeDrivesFromVolumes(vmPlan, files);

        return (vmPlan, files);

    }

    private static string? DetectArchitectureFromPath(FileInfo file, DirectoryInfo root)
    {
        var dir = file.Directory;
        if (dir == null || PathsEqual(dir.FullName, root.FullName))
            return null;

        // <hypervisor>/<processor>/<file>
        var processor = ProcessorTypes.KnownNames.FirstOrDefault(p =>
            string.Equals(p, dir.Name, StringComparison.OrdinalIgnoreCase));
        if (processor is not null)
        {
            var hypervisorDir = dir.Parent;
            var hypervisor = hypervisorDir is null
                ? null
                : Hypervisors.KnownNames.FirstOrDefault(h =>
                    string.Equals(h, hypervisorDir.Name, StringComparison.OrdinalIgnoreCase));
            if (hypervisor is not null)
                return $"{hypervisor}/{processor}";
        }

        // <hypervisor>/<file>
        var directHypervisor = Hypervisors.KnownNames.FirstOrDefault(h =>
            string.Equals(h, dir.Name, StringComparison.OrdinalIgnoreCase));
        if (directHypervisor is not null)
            return $"{directHypervisor}/any";

        return null;
    }

    private static bool PathsEqual(string a, string b) =>
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(a)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)),
            StringComparison.OrdinalIgnoreCase);

    private static void MergeDrivesFromVolumes(CatletConfig config, IEnumerable<PackableFile> files)
    {
        var existingDriveNames = (config.Drives ?? [])
            .Select(d => d.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newDrives = files
            .Where(f => f.GeneType == GeneType.Volume)
            .Select(f => f.GeneName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !existingDriveNames.Contains(name))
            .Select(name => new CatletDriveConfig { Name = name })
            .ToArray();

        if (newDrives.Length == 0)
            return;

        config.Drives = (config.Drives ?? []).Concat(newDrives).ToArray();
    }

    private static CatletConfig? ReadCatletConfig(DirectoryInfo vmExport)
    {
        var catletFile = vmExport.GetFiles("catlet.yaml", SearchOption.AllDirectories).FirstOrDefault();
        if (catletFile == null)
            return null;

        try
        {
            var content = File.ReadAllText(catletFile.FullName).Trim().Replace("\r\n", "\n");
            return CatletConfigYamlSerializer.Deserialize(content);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to read catlet.yaml file '{catletFile.FullName}'", ex);
        }
    }



    public static void ReadMetadata(DirectoryInfo vmExport, GenesetTagInfo genesetTag)
    {
        var metadataFile = vmExport.GetFiles("metadata.json", SearchOption.AllDirectories).FirstOrDefault();
        if (metadataFile == null)
        {
            return;
        }

        try
        {
            var metadata = new Dictionary<string, string>();

            using var metadataStream = metadataFile.OpenRead();
            var newMetadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataStream, GeneModelDefaults.SerializerOptions)
                              ?? metadata;
            genesetTag.JoinMetadata(newMetadata);

        }
        catch (Exception ex)
        {
            throw new Exception("failed to read metadata.json file included in exported vm", ex);
        }
    }

    private static CatletConfig? ConvertVmDataToConfig(DirectoryInfo vmExport)
    {
        var vmConfigFile = vmExport.GetFiles("vm.json", SearchOption.AllDirectories).FirstOrDefault();
        if (vmConfigFile == null)
        {
            return null;
        }
            


        try
        {
            using var vmStream = vmConfigFile.OpenRead();
            var configJson = JsonSerializer.Deserialize<JsonNode>(vmStream,GeneModelDefaults.SerializerOptions);
            if (configJson == null)
                return null;

            var vmJson = configJson["vm"];
            var firmwareJson = configJson["firmware"];
            var processorJson = configJson["processor"];
            var securityJson = configJson["security"];


            var dynamicMemory = (vmJson?["DynamicMemoryEnabled"]?.GetValue<bool>()).GetValueOrDefault();

            var capabilities = new List<CatletCapabilityConfig>();

            if (!string.IsNullOrWhiteSpace(firmwareJson?["SecureBootTemplate"]?.GetValue<string>()))
                capabilities.Add(new CatletCapabilityConfig
                {
                    Name = "secure_boot",
                    Details = ["Template:" + firmwareJson?["SecureBootTemplate"]?.GetValue<string>()]
                });

            if ((processorJson?["ExposeVirtualizationExtensions"]?.GetValue<bool>()).GetValueOrDefault())
            {
                capabilities.Add(new CatletCapabilityConfig
                {
                    Name = "nested_virtualization"
                }
                );
            }

            if ((securityJson?["TpmEnabled"]?.GetValue<bool>()).GetValueOrDefault())
            {
                string[]? details = null;
                if ((securityJson?["EncryptStateAndVmMigrationTraffic"]?.GetValue<bool>()).GetValueOrDefault())
                    details = ["with_traffic_encryption"];

                capabilities.Add(new CatletCapabilityConfig
                {
                    Name = "tpm",
                    Details = details
                }
                );
            }

            var result = new CatletConfig
            {
                Cpu = new CatletCpuConfig
                {
                    Count = vmJson?["ProcessorCount"]?.GetValue<int>()
                },
                Memory = new CatletMemoryConfig
                {
                    Startup = (int)Math.Ceiling((vmJson?["MemoryStartup"]?.GetValue<long>()).GetValueOrDefault() /
                                                1024d / 1024),
                    Maximum = dynamicMemory
                        ? (int)Math.Ceiling((vmJson?["MemoryMaximum"]?.GetValue<long>()).GetValueOrDefault() / 1024d /
                                            1024)
                        : null,
                    Minimum = dynamicMemory
                        ? (int)Math.Ceiling((vmJson?["MemoryMinimum"]?.GetValue<long>()).GetValueOrDefault() / 1024d /
                                            1024)
                        : null,
                },
                NetworkAdapters = vmJson?["NetworkAdapters"]?.AsArray().Select(adapterNode =>
                    new CatletNetworkAdapterConfig { Name = adapterNode?["Name"]?.GetValue<string>() }).ToArray(),
                Drives = vmJson?["HardDrives"]?.AsArray().Select(driveNode => new CatletDriveConfig
                {
                    Name = Path.GetFileNameWithoutExtension(driveNode?["Path"]?.GetValue<string>())
                }).ToArray(),
                Capabilities = capabilities.ToArray(),
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to convert vm.json to Catlet config", ex);
        }

    }

}