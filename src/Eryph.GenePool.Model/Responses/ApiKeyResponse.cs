using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record ApiKeyResponse(
    [property: JsonPropertyName("organization")]
    OrganizationRefResponse Organization,
    [property: JsonPropertyName("key_id")] string KeyId,
    [property: JsonPropertyName("name")] string Name);