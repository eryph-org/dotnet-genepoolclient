using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenesetDescriptionResponse(
    [property: JsonPropertyName("geneset")]
    GenesetRefResponse Geneset,
    [property: JsonPropertyName("short_description")]
    string ShortDescription,
    [property: JsonPropertyName("description")]
    string Description,
    [property: JsonPropertyName("markdown")]
    string Markdown,
    [property: JsonPropertyName("etag")] string? ETag);