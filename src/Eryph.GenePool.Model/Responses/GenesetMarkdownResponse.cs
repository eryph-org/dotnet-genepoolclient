using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetDescriptionResponse
{

    [JsonConstructor]
    public GenesetDescriptionResponse(GenesetRefResponse GenesetRefResponse,
        string ShortDescription, string Description,
        string Markdown, string? ETag)
    {
        this.Name = Name;
        this.Org = Org;
        this.Geneset = Geneset;
        this.ShortDescription = ShortDescription;
        this.Description = Description;
        this.Markdown = Markdown;
        this.ETag = ETag;
    }

    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("org")] public OrganizationRefResponse Org { get; init; }
    [JsonPropertyName("geneset")] public string Geneset { get; init; }

    [JsonPropertyName("short_description")] public string ShortDescription { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }

    [JsonPropertyName("markdown")] public string Markdown { get; init; }
    [JsonPropertyName("etag")] public string? ETag { get; init; }
}