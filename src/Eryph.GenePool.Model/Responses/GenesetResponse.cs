using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenesetResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("org")] OrganizationRefResponse Org,
    [property: JsonPropertyName("geneset")]
    string Geneset,
    [property: JsonPropertyName("uri")] Uri? Uri,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("public")] bool Public,
    [property: JsonPropertyName("short_description")]
    string ShortDescription,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("has_markdown_description")]
    bool HasMarkdown,
    [property: JsonPropertyName("description_uri")]
    Uri DescriptionUri,
    [property: JsonPropertyName("metadata")]
    IDictionary<string, string>? Metadata,
    [property: JsonPropertyName("tags_uri")]
    Uri TagsUri,
    [property: JsonPropertyName("tags")] GenesetTagResponse[]? Tags,
    [property: JsonPropertyName("tags_continuation_token")]
    string? TagsContinuationToken,
    [property: JsonPropertyName("tags_continuation_uri")]
    Uri? TagsContinuationUri,
    [property: JsonPropertyName("stats_uri")]
    Uri StatisticsUri);