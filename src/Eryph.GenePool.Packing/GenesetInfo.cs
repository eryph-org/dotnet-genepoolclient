using System.Text.Json;
using Eryph.GenePool.Model;
using LanguageExt.Common;

namespace Eryph.GenePool.Packing;

public class GenesetInfo
{
    private static readonly JsonSerializerOptions JsonSerializerOptions;

    static GenesetInfo()
    {
        JsonSerializerOptions = new JsonSerializerOptions(GeneModelDefaults.SerializerOptions)
        {
            WriteIndented = true
        };
    }

    public string GenesetName { get; }
    public string Organization { get; }
    public string Id { get; }

    private GenesetManifestData _manifestData = new();
    private readonly string _genesetPath;
    private bool _loaded;

    public GenesetManifestData ManifestData
    {
        get
        {
            EnsureLoaded();
            return _manifestData;
        }
    }

    public GenesetInfo(string geneset,string genesetPath )
    {
        geneset = geneset.ToLowerInvariant();
        var packParts = geneset.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (packParts.Length != 3 && packParts.Length != 2)
        {
            throw new ArgumentException($"invalid geneset name '{geneset}'", nameof(geneset));
        }

        Organization = packParts[0];
        Id = packParts[1];
        GenesetName = $"{Organization}/{Id}";
        _manifestData.Geneset = GenesetName;
        _manifestData.Version = GeneModelDefaults.LatestGenesetManifestVersion.ToString();
        _genesetPath = Path.GetFullPath(genesetPath);
    }

    public string GetGenesetPath()
    {
        if (File.Exists(Path.Combine(_genesetPath,"geneset.json")))
        {
            var currentManifest = ReadManifestFromPath(_genesetPath, GenesetName);
            if (currentManifest.Geneset != GenesetName)
            {
                if (currentManifest != null)
                {
                    throw new InvalidOperationException(
                        $"Directory already contains a manifest for geneset '{currentManifest.Geneset}' but you trying to access geneset '{GenesetName}'. Make sure that you are in the right folder.");
                }
                throw new InvalidOperationException(
                    $"Directory already contains a invalid manifest.");

            }

            return _genesetPath;
        }

        return Path.Combine(Organization, Id);
    }

    public bool Exists()
    {

        if (!Directory.Exists(GetGenesetPath()))
            return false;

        if (!File.Exists(Path.Combine(GetGenesetPath(), "geneset.json")))
            return false;

        return true;
    }

    public void Create()
    {
        if (Exists())
            throw new InvalidOperationException($"geneset {GenesetName} already exists in path.");

        var directoryInfo = new DirectoryInfo(GetGenesetPath());
        if (!directoryInfo.Exists)
            directoryInfo.Create();

        if (!File.Exists(Path.Combine(GetGenesetPath(), "geneset.json")))
        {
            _manifestData = new GenesetManifestData
            {
                Geneset = GenesetName,
                Version = GeneModelDefaults.LatestGenesetManifestVersion.ToString()
            };
            Write();
        }
    }

    private void EnsureCreatedAndLoaded()
    {
        if (!Exists())
            Create();
        EnsureLoaded();
    }

    private void EnsureLoaded()
    {
        if(_loaded)
            return;

        _loaded = true;
        var path = GetGenesetPath();
        _manifestData = ReadManifestFromPath(path, GenesetName);
        _manifestData.Version ??= "1.0";

    }

    public void JoinMetadata(Dictionary<string, string> newMetadata)
    {
        EnsureCreatedAndLoaded();
        _ = Validations.ValidateMetadata(true,newMetadata).ToEither().MapLeft(Error.Many).IfLeft(l => l.Throw());

        _manifestData.Metadata ??= new Dictionary<string, string>();

        _manifestData.Metadata = new[] { _manifestData.Metadata, newMetadata }
            .SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First());

        Write();
    }

    public void SetIsPublic(bool isPublic)
    {
        EnsureCreatedAndLoaded();
        _manifestData.Public = isPublic;
        Write();

    }

    public void SetShortDescription(string description)
    {
        _ = Validations.ValidateGenesetShortDescription(description).ToEither().MapLeft(Error.Many).IfLeft(l => l.Throw());

        EnsureCreatedAndLoaded();
        _manifestData.ShortDescription = description;
        Write();

    }

    public void SetMarkdown(string descriptionMarkdown)
    {
        _ = Validations.ValidateMarkdownContentSize(descriptionMarkdown).ToEither().MapLeft(Error.Many).IfLeft(l => l.Throw());

        EnsureCreatedAndLoaded();
        _manifestData.DescriptionMarkdown = descriptionMarkdown;
        _manifestData.DescriptionMarkdownFile = null;
        Write();

    }

    public void SetMarkdownFile(string descriptionMarkdownFile)
    {
        // size will be checked when reading the file
        EnsureCreatedAndLoaded();
        _manifestData.DescriptionMarkdown = null;
        _manifestData.DescriptionMarkdownFile = descriptionMarkdownFile;
        Write();

    }

    private void Write()
    {

        var jsonString = JsonSerializer.Serialize(_manifestData, JsonSerializerOptions);
        File.WriteAllText(Path.Combine(GetGenesetPath(), "geneset.json"), jsonString);
    }



    private static GenesetManifestData ReadManifestFromPath(string path, string genesetName)
    {
        try
        {
            var jsonString = File.ReadAllText(Path.Combine(path, "geneset.json"));

            var manifest = JsonSerializer.Deserialize<GenesetManifestData>(jsonString, GeneModelDefaults.SerializerOptions);
            return manifest ?? new GenesetManifestData
            {
                Geneset = genesetName,
                Version = GeneModelDefaults.LatestGenesetManifestVersion.ToString()
            };

        }
        catch
        {
            return new GenesetManifestData
            {
                Geneset = genesetName,
                Version = GeneModelDefaults.LatestGenesetManifestVersion.ToString()
            };
        }
    }


    public override string ToString()
    {
        return ToString(false);
    }

    public string ToString(bool pretty)
    {
        return JsonSerializer.Serialize(_manifestData, 
            new JsonSerializerOptions(GeneModelDefaults.SerializerOptions) { WriteIndented = pretty });
    }


    public string? GetMarkdownContent()
    {
        EnsureLoaded();
        if (_manifestData.DescriptionMarkdownFile != null)
        {
            var markdownPath = Path.Combine(GetGenesetPath(), _manifestData.DescriptionMarkdownFile);
            var fileSize = new FileInfo(markdownPath).Length;
            if (fileSize > GeneModelDefaults.MaxGenesetMarkdownBytes)
                throw new InvalidOperationException($"Markdown file '{markdownPath}' is too large. Maximum allowed size is {GeneModelDefaults.MaxGenesetMarkdownBytes / 1024 / 1024} MiB.");

            return File.ReadAllText(markdownPath);
        }

        return _manifestData.DescriptionMarkdown;
    }

    public void Validate()
    {
        EnsureLoaded();
        _ = ManifestValidations.ValidateGenesetManifest(_manifestData).ToEither()
            .MapLeft(issues => Error.New("The geneset manifest is invalid.",
                Error.Many(issues.Map(i => i.ToError()))))
            .IfLeft(e => e.Throw());

    }
}