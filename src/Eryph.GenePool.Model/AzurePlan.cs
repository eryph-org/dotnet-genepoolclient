using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

/// <summary>
/// Purchase plan for a paid / BYOL Azure marketplace image.
/// </summary>
public class AzurePlan
{
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
