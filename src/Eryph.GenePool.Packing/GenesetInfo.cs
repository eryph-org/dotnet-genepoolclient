using System.Text.Json;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing;

public class GenesetInfo
{
    public string GenesetName { get; }
    public string Organization { get; }
    public string Id { get; }

    private GenesetManifestData _manifestData = new();
    private string _genesetPath = ".";

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
            _manifestData = new GenesetManifestData { Geneset = GenesetName };
            Write();
        }
    }

    private void EnsureLoaded()
    {
        if (!Exists())
            Create();
        ReadManifest();
    }

    public void JoinMetadata(Dictionary<string, string> newMetadata)
    {
        _manifestData.Metadata ??= new Dictionary<string, string>();

        _manifestData.Metadata = new[] { _manifestData.Metadata, newMetadata }
            .SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public void SetIsPublic(bool isPublic)
    {
        EnsureLoaded();
        _manifestData.Public = isPublic;
        Write();

    }

    public void SetShortDescription(string description)
    {
        EnsureLoaded();
        _manifestData.ShortDescription = description;
        Write();

    }

    public void SetMarkdown(string descriptionMarkdown)
    {
        EnsureLoaded();
        _manifestData.DescriptionMarkdown = descriptionMarkdown;
        _manifestData.DescriptionMarkdownFile = null;
        Write();

    }

    public void SetMarkdownFile(string descriptionMarkdownFile)
    {
        EnsureLoaded();
        _manifestData.DescriptionMarkdown = null;
        _manifestData.DescriptionMarkdownFile = descriptionMarkdownFile;
        Write();

    }

    private void Write()
    {
        var jsonString = JsonSerializer.Serialize(_manifestData);
        File.WriteAllText(Path.Combine(GetGenesetPath(), "geneset.json"), jsonString);
    }

    private void ReadManifest()
    {
        var path = GetGenesetPath();
        _manifestData = ReadManifestFromPath(path, GenesetName);
    }

    private static GenesetManifestData ReadManifestFromPath(string path, string genesetName)
    {
        try
        {
            var jsonString = File.ReadAllText(Path.Combine(path, "geneset.json"));

            var manifest = JsonSerializer.Deserialize<GenesetManifestData>(jsonString);
            return manifest ?? new GenesetManifestData { Geneset = genesetName };

        }
        catch
        {
            return new GenesetManifestData { Geneset = genesetName };
        }
    }

    public override string ToString()
    {
        return ToString(false);
    }

    public string ToString(bool pretty)
    {
        return JsonSerializer.Serialize(_manifestData, new JsonSerializerOptions { WriteIndented = pretty });
    }


    public string? GetMarkdownContent()
    {
        ReadManifest();
        if (_manifestData.DescriptionMarkdownFile != null)
        {
            return File.ReadAllText(Path.Combine(GetGenesetPath(), _manifestData.DescriptionMarkdownFile));
        }

        return _manifestData.DescriptionMarkdown;
    }
}