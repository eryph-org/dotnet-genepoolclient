using Eryph.GenePool.Model;
using System.Text.Json;

namespace Eryph.GenePool.Packing;

public class GenesetTagInfo
{
    public string GenesetName { get; }
    public string Organization { get; }
    public string Id { get; }

    public string Tag { get; }

    public GenesetTagManifestData ManifestData = new();


    public GenesetTagInfo(string geneset)
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
        GenesetName = $"{Organization}/{Id}/{Tag}";
        ManifestData.Geneset = GenesetName;

    }

    public string GetGenesetPath()
    {
        if (File.Exists("geneset.json"))
        {
            var currentManifest = ReadManifestFromPath(".", GenesetName);
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

            return ".";
        }

        return Path.Combine(Organization, Id, Tag);
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
            throw new InvalidOperationException($"geneset {GenesetName} already exists.");

        var directoryInfo = new DirectoryInfo(GetGenesetPath());
        if (!directoryInfo.Exists)
            directoryInfo.Create();

        if (!File.Exists(Path.Combine(GetGenesetPath(), "geneset.json")))
        {
            ManifestData = new GenesetTagManifestData { Geneset = GenesetName };
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
        ManifestData.Metadata ??= new Dictionary<string, string>();

        ManifestData.Metadata = new[] { ManifestData.Metadata, newMetadata }
            .SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public void SetReference(string referencedGeneSet)
    {
        EnsureLoaded();
        ManifestData.Reference = referencedGeneSet;
        Write();
    }

    public void SetParent(string? parent)
    {
        EnsureLoaded();
        ManifestData.Parent = parent;
        Write();

    }

    public void AddGene(GeneType geneType, string name, string hash)
    {
        EnsureLoaded();

        switch (geneType)
        {
            case GeneType.Catlet:
                RemoveExistingGene(ManifestData.CatletGene, hash);
                ManifestData.CatletGene = hash;
                break;
            case GeneType.Volume:
                ManifestData.VolumeGenes ??= Array.Empty<GeneReferenceData>();
                RemoveExistingGene(ManifestData.VolumeGenes.FirstOrDefault(x => x.Name == name)?.Hash, hash);

                ManifestData.VolumeGenes = ManifestData.VolumeGenes.Where(x => x.Name != name)
                    .Append(new GeneReferenceData { Name = name, Hash = hash }).ToArray();
                break;
            case GeneType.Fodder:
                ManifestData.FodderGenes ??= Array.Empty<GeneReferenceData>();
                RemoveExistingGene(ManifestData.FodderGenes.FirstOrDefault(x => x.Name == name)?.Hash, hash);
                ManifestData.FodderGenes = ManifestData.FodderGenes.Where(x => x.Name != name)
                    .Append(new GeneReferenceData { Name = name, Hash = hash }).ToArray();

                break;
            default:
                throw new InvalidOperationException($"Unknown gene type {geneType}");
        }

        Write();

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
        var jsonString = JsonSerializer.Serialize(ManifestData);
        File.WriteAllText(Path.Combine(GetGenesetPath(), "geneset.json"), jsonString);
    }

    private void ReadManifest()
    {
        var path = GetGenesetPath();
        ManifestData = ReadManifestFromPath(path, GenesetName);
    }

    private static GenesetTagManifestData ReadManifestFromPath(string path, string genesetName)
    {
        try
        {
            var jsonString = File.ReadAllText(Path.Combine(path, "geneset.json"));

            var manifest = JsonSerializer.Deserialize<GenesetTagManifestData>(jsonString);
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
        return JsonSerializer.Serialize(ManifestData, new JsonSerializerOptions { WriteIndented = pretty });
    }


    public IEnumerable<string> GetAllGeneNames()
    {
        ReadManifest();
        if (!string.IsNullOrWhiteSpace(ManifestData.CatletGene))
            yield return SplitHash(ManifestData.CatletGene);

        if (ManifestData.VolumeGenes != null)
        {
            foreach (var gene in ManifestData.VolumeGenes
                         .Where(x => !string.IsNullOrWhiteSpace(x.Hash)))
            {
                yield return SplitHash(gene.Hash ?? "");
            }
        }

        if (ManifestData.FodderGenes != null)
        {
            foreach (var gene in ManifestData.FodderGenes
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
}