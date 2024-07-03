using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetMarkdownResponse
{

    [JsonConstructor]
    public GenesetMarkdownResponse(string Name, OrganizationRefResponse Org, string Geneset, string Markdown)
    {
        this.Name = Name;
        this.Org = Org;
        this.Geneset = Geneset;
        this.Markdown = Markdown;
    }

    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("org")] public OrganizationRefResponse Org { get; init; }
    [JsonPropertyName("geneset")] public string Geneset { get; init; }
    [JsonPropertyName("markdown")] public string Markdown { get; init; }
}