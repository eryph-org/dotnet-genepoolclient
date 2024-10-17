using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GetGeneDownloadResponse(
    [property: JsonPropertyName("gene")] string Gene,
    [property: JsonPropertyName("manifest")]
    GeneManifestData Manifest,
    [property: JsonPropertyName("content")]
    string? Content,
    [property: JsonPropertyName("download_uris")]
    GenePartDownloadUri[]? DownloadUris,
    [property: JsonPropertyName("download_expires")]
    DateTimeOffset DownloadExpires);