using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record ApiKeyRefResponse([property: JsonPropertyName("key_id")] string KeyId, [property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("uri")] Uri Uri)
{
    public void Deconstruct(out string keyId, out string name, out Uri uri)
    {
        keyId = KeyId;
        name = Name;
        uri = Uri;
    }
}