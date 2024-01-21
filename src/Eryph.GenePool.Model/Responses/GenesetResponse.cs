using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetResponse
{
    [JsonConstructor]
    public GenesetResponse(string Name, OrganizationRefResponse Org, string Geneset, bool Public, GenesetToTagResponse[]? Tags)
    {
        this.Name = Name;
        this.Org = Org;
        this.Geneset = Geneset;
        this.Public = Public;
        this.Tags = Tags;
    }

    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("org")] public OrganizationRefResponse Org { get; init; }

    [JsonPropertyName("geneset")]
    public string Geneset { get; init; }

    [JsonPropertyName("public")] public bool Public { get; init; }
    [JsonPropertyName("tags")] public GenesetToTagResponse[]? Tags { get; init; }

    public void Deconstruct(out string name, out OrganizationRefResponse org, out string geneset, out bool @public, out GenesetToTagResponse[]? tags)
    {
        name = Name;
        org = Org;
        geneset = Geneset;
        @public = Public;
        tags = Tags;
    }
}