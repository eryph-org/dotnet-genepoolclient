using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

/// <summary>
/// Response for geneset statistics
/// </summary>
[method: JsonConstructor]
public record GenesetStatsResponse(
    [property: JsonPropertyName("geneset")]
    GenesetRefResponse Geneset,
    [property: JsonPropertyName("status")] GenesetStatsStatus Status,
    [property: JsonPropertyName("downloads")]
    long? Downloads,
    [property: JsonPropertyName("total_size")]
    long? TotalSize,
    [property: JsonPropertyName("size")] long? Size)
{
    [JsonPropertyName("status_string")] public string StatusString => Status.ToString().ToLowerInvariant();
}