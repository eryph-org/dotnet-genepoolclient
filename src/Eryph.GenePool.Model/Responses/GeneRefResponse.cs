using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GeneRefResponse(
    [property: JsonPropertyName("geneset_tag")]
    GenesetRefResponse? GenesetTag,
    [property: JsonPropertyName("gene")] string Gene,
    [property: JsonPropertyName("uri")] Uri? Uri);