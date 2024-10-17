using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GeneUploadResponse(
    [property: JsonPropertyName("geneset")]
    string Geneset,
    [property: JsonPropertyName("gene")] string Gene,
    [property: JsonPropertyName("upload_uris")]
    GenePartUploadUri[] UploadUris);