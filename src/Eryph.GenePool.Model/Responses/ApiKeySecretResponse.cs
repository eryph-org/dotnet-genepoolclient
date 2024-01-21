using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record ApiKeySecretResponse
{
    [JsonConstructor]
    public ApiKeySecretResponse(string Organization, string KeyId, Uri Uri, string Secret)
    {
        this.Organization = Organization;
        this.KeyId = KeyId;
        this.Uri = Uri;
        this.Secret = Secret;
    }

    [JsonPropertyName("organization")]
    public string Organization { get; init; }

    [JsonPropertyName("key_id")] public string KeyId { get; init; }
    [JsonPropertyName("uri")] public Uri Uri { get; init; }
    [JsonPropertyName("secret")] public string Secret { get; init; }

    public void Deconstruct(out string organization, out string keyId, out Uri uri, out string secret)
    {
        organization = Organization;
        keyId = KeyId;
        uri = Uri;
        secret = Secret;
    }
}