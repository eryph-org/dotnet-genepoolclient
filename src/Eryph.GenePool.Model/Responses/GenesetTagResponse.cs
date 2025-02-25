using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenesetTagResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("geneset")]
    GenesetResponse? Geneset,
    [property: JsonPropertyName("tag")] string Tag,
    [property: JsonPropertyName("uri")] Uri? Uri,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("manifest")]
    GenesetTagManifestData? Manifest,
    [property: JsonPropertyName("pushed_at")]
    DateTimeOffset? PushedAt,
    [property: JsonPropertyName("total_size")]
    long? TotalSize,
    [property: JsonPropertyName("size")]
    long? Size,
    [property: JsonPropertyName("download_uri")]
    Uri? DownloadUri,
    [property: JsonPropertyName("genes_uri")]
    Uri? GenesUri,
    [property: JsonPropertyName("genes")] GetGeneResponse[]? Genes,
    [property: JsonPropertyName("genes_continuation_token")]
    string? GenesContinuationToken,
    [property: JsonPropertyName("genes_continuation_uri")]
    Uri? GenesContinuationUri);