using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

public class GeneReferenceData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }


}