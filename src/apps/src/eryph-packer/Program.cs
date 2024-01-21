
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Text.Json;
using Eryph.ConfigModel.Catlets;
using Eryph.ConfigModel.Json;
using Eryph.ConfigModel.Yaml;
using Eryph.GenePool.Client;
using Eryph.GenePool.Packing;
using Eryph.Packer;
using Spectre.Console;
using YamlDotNet.Core;

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


var genesetTagCommand = new Command("tag", "This command operates on a geneset tag.");
genesetCommand.Add(genesetTagCommand);

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

var packCommand = new Command("pack", "This command packs genes into a geneset tag");

var packVMCommand = new Command("vm", "This command packs a exported Hyper-V VM into the geneset tag.");
packVMCommand.AddArgument(genesetArgument); 
packVMCommand.AddArgument(vmExportArgument);
packCommand.AddCommand(packVMCommand);

var packCatletCommand = new Command("catlet", "This command packs a catlet gene into the geneset.");
packCatletCommand.AddArgument(genesetArgument);
packCatletCommand.AddArgument(filePathArgument);
packCommand.AddCommand(packCatletCommand);

var packVolumeCommand = new Command("volume", "This command packs a volume gene into the geneset.");
packVolumeCommand.AddArgument(genesetArgument);
packVolumeCommand.AddArgument(filePathArgument);
packCommand.AddCommand(packVolumeCommand);

var packFodderCommand = new Command("fodder", "This command packs a fodder gene into the geneset.");
packFodderCommand.AddArgument(genesetArgument);
packFodderCommand.AddArgument(filePathArgument);
packCommand.AddCommand(packFodderCommand);

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
    var genesetInfo = new GenesetInfo(genesetName);
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
    var genesetTagInfo = new GenesetTagInfo(genesetName);
    if (!genesetTagInfo.Exists())
    {
        genesetTagInfo.Create();
        Console.WriteLine(genesetTagInfo.ToString(true));
        return;
    }

    throw new InvalidOperationException("Geneset tag already initialized.");
});

// ref command
// ------------------------------
refCommand.SetHandler( context =>
{
    var genesetInfo = PrepareGeneSetTagCommand(context);
    var refPack = context.ParseResult.GetValueForArgument(refArgument);
    genesetInfo.SetReference(refPack);
    Console.WriteLine(genesetInfo.ToString(true));

});


// info command
// ------------------------------
infoGenesetCommand.SetHandler(context =>
{
    var genesetInfo = PrepareGeneSetCommand(context);
    Console.WriteLine(genesetInfo.ToString(true));

});
infoGenesetTagCommand.SetHandler(context =>
{
    var genesetInfo = PrepareGeneSetTagCommand(context);
    Console.WriteLine(genesetInfo.ToString(true));    

});


// pack vm command
// ------------------------------
packVMCommand.SetHandler(async context =>
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
            var newMetadata = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(metadataStream) 
                              ?? metadata;
            genesetInfo.JoinMetadata(newMetadata);
        }
        catch (Exception ex)
        {
            throw new Exception("failed to read metadata.json file included in exported vm", ex);
        }
    }

    var absolutePackPath = Path.GetFullPath(genesetInfo.GetGenesetPath());
    var packableFiles = await VMExport.ExportToPackable(vmExportDir, absolutePackPath, token);
    foreach (var packableFile in packableFiles)
    {
        var geneHash = await GenePacker.CreateGene(packableFile, absolutePackPath, new Dictionary<string, string>(), token);
        genesetInfo.AddGene(packableFile.GeneType, packableFile.GeneName, geneHash);
    }

    Console.WriteLine(genesetInfo.ToString(true));


});

// pack catlet command
// ------------------------------
packCatletCommand.SetHandler(async context =>
{
    var token = context.GetCancellationToken();
    var genesetInfo = PrepareGeneSetTagCommand(context);
    var catletFile = context.ParseResult.GetValueForArgument(filePathArgument);
    var absolutePackPath = Path.GetFullPath(genesetInfo.GetGenesetPath());

    var catletContent = File.ReadAllText(catletFile.FullName);
    var (jsonFile, parsedConfig) = DeserializeConfigString(catletContent);
    genesetInfo.SetParent(parsedConfig.Parent);

    if (jsonFile)
    {
        var configYaml = CatletConfigYamlSerializer.Serialize(parsedConfig);
        var catletYamlFilePath = Path.Combine(absolutePackPath, "catlet.yaml");
        await File.WriteAllTextAsync(catletYamlFilePath, configYaml);
    }
    else
    {
        File.Copy(catletFile.FullName, Path.Combine(absolutePackPath, "catlet.yaml"));
    }

    var configJson = ConfigModelJsonSerializer.Serialize(parsedConfig);
    var catletJsonFilePath = Path.Combine(absolutePackPath, "catlet.json");
    await File.WriteAllTextAsync(catletJsonFilePath, configJson);


    var packedFile =
        await GenePacker.CreateGene(
            new PackableFile(catletJsonFilePath, "catlet.json", GeneType.Catlet, "catlet", false),
            absolutePackPath, new Dictionary<string, string>(), token);

    genesetInfo.AddGene(GeneType.Catlet, "catlet", packedFile);
    Console.WriteLine(genesetInfo.ToString(true));


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
            .StartAsync($"Checking geneset {genesetTagInfo.GenesetName}", async statusContext =>
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
                    throw new Exception($"Geneset {genesetTagInfo.GenesetName} already exists on genepool.");
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
                        genesetTagInfo.GenesetName, genePath.FullName, token, progress: progress);

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
            .StartAsync($"Creating geneset {genesetTagInfo.GenesetName}", async statusContext =>
            {
                await tagClient.CreateAsync(genesetTagInfo.ManifestData, token);

            });

        AnsiConsole.WriteLine($"Geneset '{genesetTagInfo.ManifestData.Geneset}' successfully pushed to genepool.");


    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[bold red]{ex.Message}[/]");
    }

});

var commandLineBuilder = new CommandLineBuilder(rootCommand);
commandLineBuilder.UseDefaults();
commandLineBuilder.UseAnsiTerminalWhenAvailable();
commandLineBuilder.UseExceptionHandler((ex, context) =>
{
    if (ex is not OperationCanceledException)
    {
        context.Console.ResetTerminalForegroundColor();
        context.Console.SetTerminalForegroundRed();


        if (context.BindingContext.ParseResult.HasOption(debugOption))
        {
            context.Console.Error.Write(context.LocalizationResources.ExceptionHandlerHeader());
            context.Console.Error.WriteLine(ex.ToString());
        }
        else
        {
            context.Console.Error.WriteLine(ex.Message);
        }

        context.Console.ResetTerminalForegroundColor();
    }

    context.ExitCode = 1;

});

var parser = commandLineBuilder.Build();

return await parser.InvokeAsync(args);


GenesetTagInfo PrepareGeneSetTagCommand(InvocationContext context)
{
    var workDirectory = context.ParseResult.GetValueForOption(workDirOption!);
    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument!);


    if(workDirectory?.FullName != null)
        Directory.SetCurrentDirectory(workDirectory.FullName);

    var genesetInfo = new GenesetTagInfo(genesetName);
    if (!genesetInfo.Exists())
    {
        throw new InvalidOperationException($"Geneset {genesetName} not found");
    }

    return genesetInfo;
}

GenesetInfo PrepareGeneSetCommand(InvocationContext context)
{
    var workDirectory = context.ParseResult.GetValueForOption(workDirOption!);
    var genesetName = context.ParseResult.GetValueForArgument(genesetArgument!);

    if (workDirectory?.FullName != null)
        Directory.SetCurrentDirectory(workDirectory.FullName);

    var genesetInfo = new GenesetInfo(genesetName);
    if (!genesetInfo.Exists())
    {
        throw new InvalidOperationException($"Geneset {genesetName} not found");
    }

    return genesetInfo;
}

static (bool Json, CatletConfig Config) DeserializeConfigString(string configString)
{
    configString = configString.Trim();
    configString = configString.Replace("\r\n", "\n");

    if (configString.StartsWith("{") && configString.EndsWith("}"))
        return (true,CatletConfigDictionaryConverter.Convert(ConfigModelJsonSerializer.DeserializeToDictionary(configString)));

    //YAML
    try
    {
        return (false, CatletConfigYamlSerializer.Deserialize(configString));
    }
    catch (YamlException ex)
    {
        throw ex;
    }

}

public struct GeneUploadTask
{
    public string GeneName { get; set; }
    public int No { get; set; }
    public int Total { get; set; }
}