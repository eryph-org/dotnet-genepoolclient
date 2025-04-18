﻿using System.Text.Json;
using System.Text.Json.Nodes;
using Eryph.ConfigModel.Catlets;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing;

public static class VMExport
{
    public static (CatletConfig? Config, IEnumerable<PackableFile> Files) ExportToPackable(DirectoryInfo vmExport,
        CancellationToken token)
    {
        var files = new List<PackableFile>();
        var vmPlan = ConvertVmDataToConfig(vmExport) ?? new CatletConfig();

        var vhdFiles = vmExport.GetFiles("*.vhdx", SearchOption.AllDirectories);

        foreach (var vhdFile in vhdFiles)
        {
            token.ThrowIfCancellationRequested();

            files.Add(new PackableFile(vhdFile.FullName, vhdFile.Name,
                GeneType.Volume, 
                Architectures.HyperVAmd64,
                Path.GetFileNameWithoutExtension(vhdFile.Name), true, null));
        }
        
        return (vmPlan, files);

    }



    public static DirectoryInfo FindExportRootDir(DirectoryInfo vmExport)
    {
        // to make sure that we consider also if vm.json is in the parent directory
        var parent = vmExport.Parent;
        if(parent==null)
            return vmExport;

        var vmFiles = parent.GetFiles("vm.json", SearchOption.AllDirectories);
        var metadataFiles = parent.GetFiles("metadata.json", SearchOption.AllDirectories);

        if (vmFiles.Length > 0 || metadataFiles.Length > 0)
            return parent;

        return vmExport;
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