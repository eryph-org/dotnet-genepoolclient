using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record ApiKeyRefResponse([property: JsonPropertyName("key_id")] 
    string KeyId, [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_on")] DateTimeOffset CreatedOn,
    [property: JsonPropertyName("uri")] Uri Uri);