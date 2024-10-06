using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetResponse
{
    [JsonConstructor]
    public GenesetResponse(string Name, OrganizationRefResponse Org, string Geneset,
        Uri? Uri,
        string ETag,
        bool Public, string ShortDescription, string? Description, bool HasMarkdown,
        Uri DescriptionUri,
        Uri StatisticsUri,
        IDictionary<string, string> Metadata,
        Uri TagsUri,
        
        GenesetTagResponse[]? Tags, string? TagsContinuationToken, Uri? TagsContinuationUri
        )
    {
        this.Name = Name;
        this.Org = Org;
        this.Geneset = Geneset;
        this.Uri = Uri;
        this.Public = Public;
        this.ShortDescription = ShortDescription;
        this.Description = Description;
        this.HasMarkdown = HasMarkdown;
        this.DescriptionUri = DescriptionUri;
        this.StatisticsUri = StatisticsUri;
        this.Metadata = Metadata;
        this.TagsUri = TagsUri;
        this.Tags = Tags;
        this.TagsContinuationToken = TagsContinuationToken;
        this.TagsContinuationUri = TagsContinuationUri;
        this.ETag = ETag;
    }

    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("org")] public OrganizationRefResponse Org { get; init; }

    [JsonPropertyName("geneset")]
    public string Geneset { get; init; }

    [JsonPropertyName("uri")]
    public Uri? Uri { get; init; }

    [JsonPropertyName("etag")] public string? ETag { get; init; }


    [JsonPropertyName("public")] public bool Public { get; init; }

    [JsonPropertyName("short_description")] public string ShortDescription { get; init; }
    [JsonPropertyName("description_uri")] public Uri DescriptionUri { get; init; }

    [JsonPropertyName("description")] public string? Description { get; init; }

    [JsonPropertyName("has_markdown_description")] public bool HasMarkdown { get; init; }

    [JsonPropertyName("metadata")] public IDictionary<string, string> Metadata { get; init; }

    [JsonPropertyName("tags_uri")] public Uri? TagsUri { get; init; }

    [JsonPropertyName("tags")] public GenesetTagResponse[]? Tags { get; init; }
    [JsonPropertyName("tags_continuation_token")] public string? TagsContinuationToken { get; init; }
    [JsonPropertyName("tags_continuation_uri")] public Uri? TagsContinuationUri { get; init; }

    [JsonPropertyName("stats_uri")] public Uri? StatisticsUri { get; init; }

}