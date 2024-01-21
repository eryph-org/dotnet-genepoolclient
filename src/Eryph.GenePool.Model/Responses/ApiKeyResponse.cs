using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record ApiKeyResponse
{
    [JsonConstructor]
    public ApiKeyResponse(OrganizationRefResponse Organization, string KeyId, string Name)
    {
        this.Organization = Organization;
        this.KeyId = KeyId;
        this.Name = Name;
    }

    [JsonPropertyName("organization")]
    public OrganizationRefResponse Organization { get; init; }

    [JsonPropertyName("key_id")] public string KeyId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }

    public void Deconstruct(out OrganizationRefResponse organization, out string keyId, out string name)
    {
        organization = Organization;
        keyId = KeyId;
        name = Name;
    }
}