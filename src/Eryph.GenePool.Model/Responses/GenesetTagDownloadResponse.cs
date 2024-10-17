using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenesetTagDownloadResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("geneset")]
    GenesetResponse? Geneset,
    [property: JsonPropertyName("tag")] string Tag,
    [property: JsonPropertyName("manifest")]
    GenesetTagManifestData? Manifest,
    [property: JsonPropertyName("genes")] GetGeneDownloadResponse[]? Genes);