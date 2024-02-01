
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Text.Json;
using Azure;
using Eryph.ConfigModel.Catlets;
using Eryph.ConfigModel.FodderGenes;
using Eryph.ConfigModel.Json;
using Eryph.ConfigModel.Yaml;
using Eryph.GenePool.Client;
using Eryph.GenePool.Model;
using Eryph.GenePool.Packing;
using Eryph.Packer;
using Eryph.Packer.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using Command = System.CommandLine.Command;
using GeneType = Eryph.GenePool.Packing.GeneType;

//AnsiConsole.Profile.Capabilities.Interactive = false;

var genesetArgument = new Argument<string>("geneset", "name of geneset in format organization/id/[tag]");
var refArgument = new Argument<string>("referenced geneset", "name of referenced geneset in format organization/id/tag");
var vmExportArgument = new Argument<DirectoryInfo>("vm export", "path to exported VM");
var filePathArgument = new Argument<FileInfo>("file", "path to file");

var isPublicOption = new Option<bool>("--public", "sets genesets visibility to public");
var shortDescriptionOption = new Option<string>("--description", "sets genesets description");


vmExportArgument.ExistingOnly();

var debugOption =
    new Option<bool>("--debug", "Enables debug output.");

var apiKeyOption =
    new Option<string>("--api-key", "API key for authentication");

var workDirOption =
    // ReSharper disable once StringLiteralTypo
    new Option<DirectoryInfo>("--workdir", "work directory")
        .ExistingOnly();
    
workDirOption.SetDefaultValue(new DirectoryInfo(Environment.CurrentDirectory));


var rootCommand = new RootCommand();
rootCommand.AddGlobalOption(workDirOption);
rootCommand.AddGlobalOption(debugOption);

var genesetCommand = new Command("geneset", "This command operates on a geneset.");
rootCommand.Add(genesetCommand);

var infoGenesetCommand = new Command("info", "This command reads the metadata of a geneset.");
infoGenesetCommand.AddArgument(genesetArgument);

genesetCommand.Add(infoGenesetCommand);

var initGenesetCommand =
    new Command("init", "This command initializes the filesystem structure for a geneset.");
initGenesetCommand.AddArgument(genesetArgument);
initGenesetCommand.AddOption(isPublicOption);
initGenesetCommand.AddOption(shortDescriptionOption);

genesetCommand.Add(initGenesetCommand);


var genesetTagCommand = new Command("geneset-tag", "This command operates on a geneset tag.");
rootCommand.Add(genesetTagCommand);

var infoGenesetTagCommand = new Command("info", "This command reads the metadata of a geneset tag.");
infoGenesetTagCommand.AddArgument(genesetArgument);
genesetTagCommand.Add(infoGenesetTagCommand);

var initGenesetTagCommand =
    new Command("init", "This command initializes the filesystem structure for a geneset tag.");
initGenesetTagCommand.AddArgument(genesetArgument);
genesetTagCommand.Add(initGenesetTagCommand);

var refCommand = new Command("ref", "This command adds a reference to another geneset to the geneset tag.");
refCommand.AddArgument(genesetArgument);
refCommand.AddArgument(refArgument);
genesetTagCommand.Add(refCommand);


var addVMCommand = new Command("add-vm", "This command adds a exported Hyper-V VM to the geneset tag.");
addVMCommand.AddArgument(genesetArgument);
addVMCommand.AddArgument(vmExportArgument);
genesetTagCommand.AddCommand(addVMCommand);

var addVolumeCommand = new Command("add-volume", "This command adds a volume reference to the geneset tag.");
addVolumeCommand.AddArgument(genesetArgument);
addVolumeCommand.AddArgument(filePathArgument);
genesetTagCommand.AddCommand(addVolumeCommand);


var packCommand = new Command("pack", "This command packs the content of the local geneset tag into genes");
packCommand.AddArgument(genesetArgument);

genesetTagCommand.Add(packCommand);

var pushCommand = new Command("push", "This command uploads a geneset to eryph genepool");
pushCommand.AddArgument(genesetArgument);
pushCommand.AddOption(apiKeyOption);
genesetTagCommand.Add(pushCommand);



// init commands
// ------------------------------
initGenesetCommand.SetHandler( context =>
{
    var workdir = context.ParseResult.GetValueForOption(workDirOption);
    var isPublic = context.ParseResult.GetValueForOption(isPublicOption);
    var description = context.ParseResult.GetValueForOption(shortDescriptionOption);

    if (workdir?.FullName != null)
        Directory.SetCurrentDirectory(workdir.FullName);

    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument);
    var genesetInfo = new GenesetInfo(genesetName, ".");
    if (!genesetInfo.Exists())
    {
        genesetInfo.Create();
        if(File.Exists(Path.Combine(genesetInfo.GetGenesetPath(), "readme.md")))
        {
            genesetInfo.SetMarkdownFile("readme.md");
        }

        genesetInfo.SetIsPublic(isPublic);

        if (!string.IsNullOrWhiteSpace(description))
            genesetInfo.SetShortDescription(description);

        AnsiConsole.Write(new Rows(
            new Text("Geneset was initialized:"),
            new JsonText(genesetInfo.ToString())));

        return;
    }

    throw new InvalidOperationException("Geneset already initialized.");
});

initGenesetTagCommand.SetHandler(context =>
{
    var workdir = context.ParseResult.GetValueForOption(workDirOption);

    if (workdir?.FullName != null)
        Directory.SetCurrentDirectory(workdir.FullName);

    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument);
    var genesetTagInfo = new GenesetTagInfo(genesetName, ".");
    if (!genesetTagInfo.Exists())
    {
        genesetTagInfo.Create();
        AnsiConsole.Write(new Rows(
            new Text("Geneset tag was initialized:"),
            new JsonText(genesetTagInfo.ToString(false))));
        return;
    }

    throw new InvalidOperationException("Geneset tag already initialized.");
});

// ref command
// ------------------------------
refCommand.SetHandler(context =>
{
    var genesetInfo = PrepareGeneSetTagCommand(context);
    var refPack = context.ParseResult.GetValueForArgument(refArgument);
    genesetInfo.SetReference(refPack);
    AnsiConsole.Write(new Rows(
        new Text("Reference was added to the geneset tag:"),
        new JsonText(genesetInfo.ToString(false))));
});


// info command
// ------------------------------
infoGenesetCommand.SetHandler(context =>
{
    var genesetInfo = PrepareGeneSetCommand(context);
    AnsiConsole.Write(new JsonText(genesetInfo.ToString()));

});

infoGenesetTagCommand.SetHandler(context =>
{
    var genesetTagInfo = PrepareGeneSetTagCommand(context);
    AnsiConsole.Write(new JsonText(genesetTagInfo.ToString()));
});


// add vm command
// ------------------------------
addVMCommand.SetHandler(async context =>
{
    var token = context.GetCancellationToken();
    var genesetInfo = PrepareGeneSetTagCommand(context);
    var vmExportDir = context.ParseResult.GetValueForArgument(vmExportArgument);
    var metadata = new Dictionary<string, string>();
    var metadataFile = new FileInfo(Path.Combine(vmExportDir.FullName, "metadata.json"));
    if (metadataFile.Exists)
    {
        try
        {
            await using var metadataStream = metadataFile.OpenRead();
            var newMetadata = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(metadataStream, GeneModelDefaults.SerializerOptions) 
                              ?? metadata;
            genesetInfo.JoinMetadata(newMetadata);
        }
        catch (Exception ex)
        {
            throw new Exception("failed to read metadata.json file included in exported vm", ex);
        }
    }

    var absolutePackPath = Path.GetFullPath(genesetInfo.GetGenesetPath());
    var (config, packableFiles) = VMExport.ExportToPackable(vmExportDir, token);
    
    var configYaml = CatletConfigYamlSerializer.Serialize(config);
    var catletYamlFilePath = Path.Combine(absolutePackPath, "catlet.yaml");
    await File.WriteAllTextAsync(catletYamlFilePath, configYaml);

    ResetPackableFolder(absolutePackPath);
    WritePackableFiles(packableFiles, absolutePackPath);
    
    AnsiConsole.Write(new JsonText(genesetInfo.ToString()));
});

// pack catlet command
// ------------------------------
packCommand.SetHandler(async context =>
{
    var packedGenesetInfo = await AnsiConsole.Progress()
        .HideCompleted(false)
        .Columns(
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn { Spinner = Spinner.Known.Pong })
        .StartAsync(async progressContext =>
        {
            var prepareCatletProgressTask = progressContext.AddTask("Preparing catlet", autoStart: true);
            var prepareFodderProgressTask = progressContext.AddTask("Preparing fodder", autoStart: false);
            var packingGenesetProgressTask = progressContext.AddTask("Packing geneset", autoStart: false);

            var token = context.GetCancellationToken();
            var genesetTagInfo = PrepareGeneSetTagCommand(context);
            var absoluteGenesetPath = Path.GetFullPath(genesetTagInfo.GetGenesetPath());

            // folder .pack is the temporary folder for packing
            var catletFile = Path.Combine(absoluteGenesetPath, "catlet.yaml");
            var packFolder = Path.Combine(absoluteGenesetPath, ".pack");
            if (!Directory.Exists(packFolder))
                Directory.CreateDirectory(packFolder);


            var packableFiles = await ReadPackableFiles(absoluteGenesetPath);
            string? parent = null;
            // pack catlet
            if (File.Exists(catletFile))
            {
                var catletContent = File.ReadAllText(catletFile);
                var catletConfig = DeserializeCatletConfigString(catletContent);
                var configJson = ConfigModelJsonSerializer.Serialize(catletConfig);
                await File.WriteAllTextAsync(Path.Combine(packFolder, "catlet.json"), configJson);
                packableFiles.Add(new PackableFile(Path.Combine(packFolder, "catlet.json"),
                    "catlet.json", GeneType.Catlet, "catlet", false));
                parent = catletConfig.Parent;
            }

            prepareCatletProgressTask.Value = 100;
            prepareCatletProgressTask.StopTask();

            prepareFodderProgressTask.StartTask();
            // pack fodder
            var fodderDir = new DirectoryInfo(Path.Combine(absoluteGenesetPath, "fodder"));
            if (fodderDir.Exists)
            {
                foreach (var fodderFile in fodderDir.GetFiles("*.*").Where(x =>
                         {
                             var extension = Path.GetExtension(x.Name).ToLowerInvariant();
                             return extension is ".yaml" or ".yml";
                         }))
                {
                    var fodderContent = File.ReadAllText(fodderFile.FullName);
                    var fodderConfig = DeserializeFodderConfigString(fodderContent);
                    var fodderJson = ConfigModelJsonSerializer.Serialize(fodderConfig);
                    var fodderPackFolder = Path.Combine(packFolder, "fodder");
                    if (!Directory.Exists(fodderPackFolder))
                        Directory.CreateDirectory(fodderPackFolder);

                    var fodderJsonFile = Path.Combine(fodderPackFolder, $"{fodderConfig.Name}.json");
                    await File.WriteAllTextAsync(fodderJsonFile, fodderJson);
                    packableFiles.Add(new PackableFile(fodderJsonFile,
                        $"{fodderConfig.Name}.json", GeneType.Fodder, fodderConfig.Name, false));

                }
            }

            prepareFodderProgressTask.Value = 100;
            prepareFodderProgressTask.StopTask();


            packingGenesetProgressTask.MaxValue = packableFiles.Count;
            packingGenesetProgressTask.StartTask();

            // created .packed folder with packing result
            var packedFolder = Path.Combine(absoluteGenesetPath, ".packed");
            if (Directory.Exists(packedFolder))
                Directory.Delete(packedFolder, true);
            Directory.CreateDirectory(packedFolder);
            genesetTagInfo.PreparePacking();
            File.Copy(Path.Combine(absoluteGenesetPath, "geneset-tag.json"),
                Path.Combine(packedFolder, "geneset-tag.json"));
            var packedGenesetInfo = new GenesetTagInfo(genesetTagInfo.GenesetTagName, packedFolder);

            packedGenesetInfo.SetParent(parent);

            var packingTasks = packableFiles.Select(pf => (
                Packable: pf,
                ProgressTask: progressContext.AddTask($"Packing gene {pf.GeneName}", autoStart: false)
                )).ToList();

            // this will pack all genes in .packed folder
            foreach (var packingTask in packingTasks)
            {
                packingTask.ProgressTask.StartTask();
                var progress = new Progress<GenePackerProgress>();
                progress.ProgressChanged += (_, progressData) =>
                {
                    packingTask.ProgressTask.MaxValue = progressData.TotalBytes;
                    packingTask.ProgressTask.Value = progressData.ProcessedBytes;
                };
                var packedFile = await GenePacker.CreateGene(packingTask.Packable, packedFolder, progress, token);
                packedGenesetInfo.AddGene(packingTask.Packable.GeneType, packingTask.Packable.GeneName, packedFile);
                packingGenesetProgressTask.Increment(1);
            }

            // remove the temporary .pack folder
            if (Directory.Exists(packFolder))
                Directory.Delete(packFolder, true);

            packingGenesetProgressTask.StopTask();

            return packedGenesetInfo;

            static CatletConfig DeserializeCatletConfigString(string configString)
            {
                configString = configString.Trim();
                configString = configString.Replace("\r\n", "\n");

                return CatletConfigYamlSerializer.Deserialize(configString);
            }

            static FodderGeneConfig DeserializeFodderConfigString(string configString)
            {
                configString = configString.Trim();
                configString = configString.Replace("\r\n", "\n");

                return FodderGeneConfigYamlSerializer.Deserialize(configString);
            }
        });

    AnsiConsole.Write(new JsonText(packedGenesetInfo.ToString()));
});


// push command
// ------------------------------
pushCommand.SetHandler(async context =>
{
    var token = context.GetCancellationToken();
    var genesetInfo = PrepareGeneSetCommand(context);
    var genesetTagInfo = PrepareGeneSetTagCommand(context);
    var isInteractive = AnsiConsole.Profile.Capabilities.Interactive;
    var apiKey = context.ParseResult.GetValueForOption(apiKeyOption);

    var packedFolder = Path.Combine(genesetTagInfo.GetGenesetPath(), ".packed");
    if (!Directory.Exists(packedFolder))
        throw new InvalidOperationException($"Geneset tag '{genesetTagInfo.GenesetTagName}' is not packed. Use 'pack' command first.");

    genesetTagInfo = new GenesetTagInfo(genesetTagInfo.GenesetTagName, packedFolder);
    
    try
    {
        var credential = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Authenticating to genepool...",async statusContext =>
            {
                statusContext.Status = "Authenticated to genepool.";
                statusContext.Refresh();
                return await AuthProvider.GetCredential(apiKey);
            });

        var genePoolUri = new Uri("https://eryphgenepoolapistaging.azurewebsites.net/api/");

        var genePoolClient = credential.ApiKey != null
            ? new GenePoolClient(genePoolUri, credential.ApiKey)
            : new GenePoolClient(genePoolUri, credential.Token!);

        var genesetClient = genePoolClient.GetGenesetClient(genesetTagInfo.Organization, genesetTagInfo.Id);
        var tagClient = genesetClient.GetGenesetTagClient(genesetTagInfo.Tag);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync($"Checking geneset tag {genesetTagInfo.GenesetTagName}", async statusContext =>
            {
                var markdownContent = genesetInfo.GetMarkdownContent();

                if (!await genesetClient.ExistsAsync())
                {
                    statusContext.Status = $"Creating geneset {genesetInfo.GenesetName}";
                    statusContext.Refresh();
                    await genesetClient.CreateAsync(genesetInfo.ManifestData.Public ?? false,
                        genesetInfo.ManifestData.ShortDescription,
                        markdownContent, token
                    );
                }

                if (await tagClient.ExistsAsync())
                {
                    throw new Exception($"Geneset {genesetTagInfo.GenesetTagName} already exists on genepool.");
                }

            });


        var packDir = new DirectoryInfo(genesetTagInfo.GetGenesetPath());
        var allGenes = genesetTagInfo.GetAllGeneNames().ToArray();

        await AnsiConsole.Progress()
            .HideCompleted(false)
            .Columns(new TaskGeneColumn(), 
                new TaskGeneCountColumn(), 
                new TaskDescriptionColumn{Alignment = Justify.Left}, 
                new ProgressBarColumn(), 
                new PercentageColumn(), 
                new SpinnerColumn{Spinner = Spinner.Known.Pong}).StartAsync(async progressContext =>
            {
                var count = 0;
                foreach (var geneName in allGenes)
                {
                    count++;
                    var genePath = new DirectoryInfo(Path.Combine(packDir.FullName, geneName));
                    if (!genePath.Exists)
                        throw new Exception($"Gene {geneName} not found in directory {packDir.FullName}");

                    var progressLock = new object();
                    var progress = new Progress<GeneUploadProgress>();
                    ProgressTask? progressTask = null;
                    var state = new GeneUploadTask
                    {
                        GeneName = $"Gene {geneName[..12]}",
                        No = count,
                        Total = allGenes.Length
                    };
                    var baseDescription = !isInteractive
                        ? $"Gene {geneName[..12]}: {count} of {allGenes.Length} - "
                        : "";

                    progress.ProgressChanged += (_, progressData) =>
                    {
                        var totalSizeScale = "Bytes";
                        var totalSize = progressData.TotalMissingSize * 1d;
                        var uploadedSize = progressData.TotalUploadedSize * 1d;
                        if (totalSize > 10 * 1024)
                        {
                            totalSizeScale = "KB";
                            totalSize /= 1024d;
                            uploadedSize /= 1024d;
                        }

                        if (totalSize > 10 * 1024)
                        {
                            totalSizeScale = "MB";
                            totalSize /= 1024d;
                            uploadedSize /= 1024d;
                        }

                        if (totalSize > 10 * 1024)
                        {
                            totalSizeScale = "GB";
                            totalSize /= 1024d;
                            uploadedSize /= 1024d;
                        }

                        lock(progressLock)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            if (progressTask == null)
                                progressTask =
                                    progressContext.AddTask(
                                        $"{baseDescription}0.00 {totalSizeScale} of {totalSize:0.00} {totalSizeScale})",
                                        maxValue: totalSize);

                            progressTask.State.Update<GeneUploadTask>("gene", _ => state);
                            progressTask.Value=uploadedSize;
                            progressTask.Description =
                                $"{baseDescription}{uploadedSize:0.00} {totalSizeScale} of {totalSize:0.00} {totalSizeScale}";
                        }
                    };

                    await genePoolClient.CreateGeneFromPathAsync(
                        genesetTagInfo.GenesetTagName, genePath.FullName, token, progress: progress);

                    if (progressTask == null && isInteractive)
                    {
                        
                        progressTask =
                            progressContext.AddTask(
                                $"{baseDescription}upload completed");
                        progressTask.Value = 100;
                        progressTask.State.Update<GeneUploadTask>("gene", _ => state);

                    }

                    if (progressTask != null)
                    {
                        progressTask.Description =
                            $"{baseDescription}upload completed";
                        progressTask?.StopTask();
                    }
                };

            });


        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync($"Creating geneset tag {genesetTagInfo.GenesetTagName}", async statusContext =>
            {
                await tagClient.CreateAsync(genesetTagInfo.ManifestData, token);

            });

        AnsiConsole.WriteLine($"Geneset tag '{genesetTagInfo.ManifestData.Geneset}' successfully pushed to genepool.");

    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }

});

var commandLineBuilder = new CommandLineBuilder(rootCommand);
commandLineBuilder.UseDefaults();
commandLineBuilder.UseAnsiTerminalWhenAvailable();
commandLineBuilder.UseExceptionHandler((ex, context) =>
{
    if (ex is not OperationCanceledException)
    {
        AnsiConsole.WriteException(ex);
    }

    context.ExitCode = 1;
});

var parser = commandLineBuilder.Build();

return await parser.InvokeAsync(args);

void WritePackableFiles(IEnumerable<PackableFile> files, string genesetPath)
{
    var packFolder = Path.Combine(genesetPath, ".pack");
    if (!Directory.Exists(packFolder))
        Directory.CreateDirectory(packFolder);
    
    var packableJson = JsonSerializer.Serialize(files, GeneModelDefaults.SerializerOptions);
    var packableJsonFilePath = Path.Combine(packFolder, "packable.json");
    File.WriteAllText(packableJsonFilePath, packableJson);
}

void ResetPackableFolder(string genesetPath)
{
    var packFolder = Path.Combine(genesetPath, ".pack");
    if (Directory.Exists(packFolder))
        Directory.Delete(packFolder, true);

}

async Task<List<PackableFile>> ReadPackableFiles(string genesetPath)
{
    var packFolder = Path.Combine(genesetPath, ".pack");

    if (File.Exists(Path.Combine(packFolder, "packable.json")))
    {
        var packableJson = await File.ReadAllTextAsync(Path.Combine(packFolder, "packable.json"));
        return JsonSerializer.Deserialize<List<PackableFile>>(packableJson, GeneModelDefaults.SerializerOptions) ?? new List<PackableFile>();
    }

    return new List<PackableFile>();
}

GenesetTagInfo PrepareGeneSetTagCommand(InvocationContext context)
{
    var workDirectory = context.ParseResult.GetValueForOption(workDirOption!);
    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument!);


    if(workDirectory?.FullName != null)
        Directory.SetCurrentDirectory(workDirectory.FullName);

    var genesetInfo = new GenesetTagInfo(genesetName, ".");
    if (!genesetInfo.Exists())
    {
        throw new InvalidOperationException($"Geneset tag {genesetName} not found");
    }

    return genesetInfo;
}

GenesetInfo PrepareGeneSetCommand(InvocationContext context)
{
    var workDirectory = context.ParseResult.GetValueForOption(workDirOption!);
    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument!);

    if (workDirectory?.FullName != null)
        Directory.SetCurrentDirectory(workDirectory.FullName);

    var genesetInfo = new GenesetInfo(genesetName, ".");
    if (!genesetInfo.Exists())
    {
        throw new InvalidOperationException($"Geneset {genesetName} not found");
    }

    return genesetInfo;
}

public struct GeneUploadTask
{
    public string GeneName { get; set; }
    public int No { get; set; }
    public int Total { get; set; }
}
