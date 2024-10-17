using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GeneUploadStatusResponse(
    [property: JsonPropertyName("uploaded_size")]
    long UploadedSize,
    [property: JsonPropertyName("uploaded_parts")]
    string[] UploadedParts,
    [property: JsonPropertyName("confirmed_parts")]
    string[] ConfirmedParts);