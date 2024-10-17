using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GetGeneContentResponse([property: JsonPropertyName("gene")] GeneRefResponse? Gene,
    [property: JsonPropertyName("content")]
    string? Content,
    [property: JsonPropertyName("readable_content")]
    string? ReadableContent);