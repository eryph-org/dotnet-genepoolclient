using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetResponse
{
    [JsonConstructor]
    public GenesetResponse(string Name, OrganizationRefResponse Org, string Geneset, string ETag, 
        bool Public, string ShortDescription, string? Description, bool HasMarkdown,
        IDictionary<string, string> Metadata,
        GenesetToTagResponse[]? Tags)
    {
        this.Name = Name;
        this.Org = Org;
        this.Geneset = Geneset;
        this.Public = Public;
        this.ShortDescription = ShortDescription;
        this.Description = Description;
        this.HasMarkdown = HasMarkdown;
        this.Metadata = Metadata;
        this.Tags = Tags;
        this.ETag = ETag;
    }

    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("org")] public OrganizationRefResponse Org { get; init; }

    [JsonPropertyName("geneset")]
    public string Geneset { get; init; }

    [JsonPropertyName("etag")] public string? ETag { get; init; }


    [JsonPropertyName("public")] public bool Public { get; init; }

    [JsonPropertyName("short_description")] public string ShortDescription { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }

    [JsonPropertyName("has_markdown_description")] public bool HasMarkdown { get; init; }
    [JsonPropertyName("metadata")] public IDictionary<string,string> Metadata { get; init; }


    [JsonPropertyName("tags")] public GenesetToTagResponse[]? Tags { get; init; }



    public void Deconstruct(out string name, out OrganizationRefResponse org, out string geneset, out string?etag, 
        
        out bool @public,
        out string? shortDescription,
        out string? description,
        out bool hasMarkdown,
        out IDictionary<string, string> metadata, out GenesetToTagResponse[]? tags)
    {
        name = Name;
        org = Org;
        geneset = Geneset;
        etag = ETag;
        @public = Public;
        shortDescription = ShortDescription;
        description = Description;
        hasMarkdown = HasMarkdown;
        metadata = Metadata;
        tags = Tags;
    }
}