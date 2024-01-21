using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetTagResponse
{
    [JsonConstructor]
    public GenesetTagResponse(string Name, GenesetResponse Geneset, string Tag, GenesetTagManifestData Manifest)
    {
        this.Name = Name;
        this.Geneset = Geneset;
        this.Tag = Tag;
        this.Manifest = Manifest;
    }

    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("geneset")]
    public GenesetResponse Geneset { get; init; }

    [JsonPropertyName("tag")] public string Tag { get; init; }

    [JsonPropertyName("manifest")]
    public GenesetTagManifestData Manifest { get; init; }

    public void Deconstruct(out string name, out GenesetResponse geneset, out string tag, out GenesetTagManifestData manifest)
    {
        name = Name;
        geneset = Geneset;
        tag = Tag;
        manifest = Manifest;
    }
}