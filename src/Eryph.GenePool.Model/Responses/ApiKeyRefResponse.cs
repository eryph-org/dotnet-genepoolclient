using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record ApiKeyRefResponse
{
    [JsonConstructor]
    public ApiKeyRefResponse(string KeyId, string Name, Uri Uri)
    {
        this.KeyId = KeyId;
        this.Name = Name;
        this.Uri = Uri;
    }

    [JsonPropertyName("key_id")] public string KeyId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("uri")] public Uri Uri { get; init; }

    public void Deconstruct(out string keyId, out string name, out Uri uri)
    {
        keyId = KeyId;
        name = Name;
        uri = Uri;
    }
}