using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.ApiKeys;

public class CreateApiKeyBody
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("permissions")]
    public string[]? Permissions { get; set; }
}