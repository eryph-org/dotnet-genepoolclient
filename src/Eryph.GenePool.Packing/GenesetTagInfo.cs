using Eryph.GenePool.Model;
using System.Text.Json;
using Error = LanguageExt.Common.Error;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Eryph.GenePool.Packing;

public class GenesetTagInfo
{
    public string GenesetTagName { get; }
    public string Organization { get; }
    public string Id { get; }

    public string Tag { get; }

    private GenesetTagManifestData _manifestData;
    private readonly string _genesetPath;
    private bool _loaded;

    public GenesetTagManifestData ManifestData
    {
        get
        {
            EnsureLoaded();
            return _manifestData;
        }
    }

    public GenesetTagInfo(string geneset, string genesetPath)
    {
        geneset = geneset.ToLowerInvariant();
        var packParts = geneset.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (packParts.Length != 3 && packParts.Length != 2)
        {
            throw new ArgumentException($"invalid geneset name '{geneset}'", nameof(geneset));
        }

        Organization = packParts[0];
        Id = packParts[1];
        Tag = packParts.Length == 3 ? packParts[2] : "latest";
        GenesetTagName = $"{Organization}/{Id}/{Tag}";

        _manifestData = new GenesetTagManifestData
            {
            Version = GeneModelDefaults.LatestGenesetTagManifestVersion.ToString(),
            Geneset = GenesetTagName
        };
        _genesetPath = Path.GetFullPath(genesetPath);
    }

    public string GetGenesetPath()
    {
        if (File.Exists(Path.Combine(_genesetPath,"geneset-tag.json")))
        {
            var currentManifest = ReadManifestFromPath(_genesetPath, GenesetTagName);
            if (currentManifest.Geneset != GenesetTagName)
            {
                if (currentManifest != null)
                {
                    throw new InvalidOperationException(
                        $"Directory already contains a manifest for geneset tag '{currentManifest.Geneset}' but you are trying to access geneset tag '{GenesetTagName}'. Make sure that you are in the correct directory.");
                }
                throw new InvalidOperationException(
                    "Directory already contains a invalid manifest.");

            }

            return _genesetPath;
        }

        return Path.Combine(Organization, Id, Tag);
    }

    public bool Exists()
    {

        if (!Directory.Exists(GetGenesetPath()))
            return false;

        if (!File.Exists(Path.Combine(GetGenesetPath(), "geneset-tag.json")))
            return false;

        return true;
    }

    public bool IsReference() => !string.IsNullOrWhiteSpace(ManifestData.Reference);

    public void Create()
    {
        if (Exists())
            throw new InvalidOperationException($"geneset tag {GenesetTagName} already exists.");

        var directoryInfo = new DirectoryInfo(GetGenesetPath());
        if (!directoryInfo.Exists)
            directoryInfo.Create();

        if (!File.Exists(Path.Combine(GetGenesetPath(), "geneset-tag.json")))
        {
            _manifestData = new GenesetTagManifestData { 
                Version = GeneModelDefaults.LatestGenesetTagManifestVersion.ToString(),
                Geneset = GenesetTagName };
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
        if (_loaded)
            return;

        _loaded = true;
        var path = GetGenesetPath();
        _manifestData = ReadManifestFromPath(path, GenesetTagName);
        _manifestData.Version ??= "1.0";

    }


    public void JoinMetadata(Dictionary<string, string> newMetadata)
    {
        EnsureCreatedAndLoaded();

        _ = Validations.ValidateMetadata(false, newMetadata).ToEither().MapLeft(Error.Many).IfLeft(l => l.Throw());

        _manifestData.Metadata ??= new Dictionary<string, string>();

        _manifestData.Metadata = new[] { _manifestData.Metadata, newMetadata }
            .SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First());
        Write();
    }

    public void SetReference(string referencedGeneSet)
    {
        EnsureCreatedAndLoaded();
        _manifestData.Reference = referencedGeneSet;
        Write();
    }

    public void SetParent(string? parent)
    {
        EnsureCreatedAndLoaded();
        _manifestData.Parent = parent;
        Write();

    }

    public void AddGene(GeneType geneType, string name, string hash, string architecture)
    {
        EnsureCreatedAndLoaded();

        switch (geneType)
        {
            case GeneType.Catlet:
                RemoveExistingGene(_manifestData.CatletGene, hash);
                _manifestData.CatletGene = hash;
                break;
            case GeneType.Volume:
                _manifestData.VolumeGenes ??= [];
                RemoveExistingGene(_manifestData.VolumeGenes.FirstOrDefault(x => x.Name == name && x.Architecture == architecture)?.Hash, hash);

                _manifestData.VolumeGenes = _manifestData.VolumeGenes.Where(x => x.Name != name || x.Architecture != architecture)
                    .Append(new GeneReferenceData { Name = name, Hash = hash, Architecture = architecture}).ToArray();
                break;
            case GeneType.Fodder:
                _manifestData.FodderGenes ??= [];
                RemoveExistingGene(_manifestData.FodderGenes.FirstOrDefault(x => x.Name == name && x.Architecture == architecture)?.Hash, hash);
                _manifestData.FodderGenes = _manifestData.FodderGenes.Where(x => x.Name != name || x.Architecture != architecture)
                    .Append(new GeneReferenceData { Name = name, Hash = hash, Architecture = architecture}).ToArray();

                break;
            default:
                throw new InvalidOperationException($"Unknown gene type {geneType}");
        }

        Write();

    }

    public void Validate()
    {
        EnsureLoaded();
        _ = ManifestValidations.ValidateGenesetTagManifest(_manifestData).ToEither()
                .MapLeft(issues => Error.New("The geneset tag manifest is invalid.",
                    Error.Many(issues.Map(i => i.ToError()))))
                .IfLeft(e => e.Throw());

    }

    private void RemoveExistingGene(string? hashName, string newHash)
    {
        if (string.IsNullOrWhiteSpace(hashName) || hashName == newHash) return;

        var folderName = hashName.Split(":")[1];
        var geneDir = Path.Combine(GetGenesetPath(), folderName);

        if (Directory.Exists(geneDir))
            Directory.Delete(geneDir, true);

    }


    private void Write()
    {
        var jsonString = JsonSerializer.Serialize(_manifestData, GeneModelDefaults.SerializerOptions);
        File.WriteAllText(Path.Combine(GetGenesetPath(), "geneset-tag.json"), jsonString);
    }

    private static GenesetTagManifestData ReadManifestFromPath(string path, string genesetName)
    {
        try
        {
            var jsonString = File.ReadAllText(Path.Combine(path, "geneset-tag.json"));

            var manifest = JsonSerializer.Deserialize<GenesetTagManifestData>(jsonString, GeneModelDefaults.SerializerOptions);
            return manifest ?? new GenesetTagManifestData { Geneset = genesetName };

        }
        catch
        {
            return new GenesetTagManifestData { Geneset = genesetName };
        }
    }

    public override string ToString()
    {
        return ToString(false);
    }

    public string ToString(bool pretty)
    {
        EnsureLoaded();
        return JsonSerializer.Serialize(_manifestData, 
            new JsonSerializerOptions(GeneModelDefaults.SerializerOptions) { WriteIndented = pretty });
    }


    public IEnumerable<string> GetAllGeneNames()
    {
        EnsureLoaded();
        if (!string.IsNullOrWhiteSpace(_manifestData.CatletGene))
            yield return SplitHash(_manifestData.CatletGene);

        if (_manifestData.VolumeGenes != null)
        {
            foreach (var gene in _manifestData.VolumeGenes
                         .Where(x => !string.IsNullOrWhiteSpace(x.Hash)))
            {
                yield return SplitHash(gene.Hash ?? "");
            }
        }

        if (_manifestData.FodderGenes != null)
        {
            foreach (var gene in _manifestData.FodderGenes
                         .Where(x => !string.IsNullOrWhiteSpace(x.Hash)))
            {
                yield return SplitHash(gene.Hash ?? "");
            }
        }
    }

    private static string SplitHash(string hash)
    {
        var parts = hash.Split(':');
        return parts.Length != 2 ? hash : parts[1];
    }

    public void PreparePacking()
    {
        EnsureCreatedAndLoaded();
        _manifestData.CatletGene = null;
        _manifestData.VolumeGenes = null;
        _manifestData.FodderGenes = null;
        Write();
    }
}