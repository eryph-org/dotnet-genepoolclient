using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record ApiKeyResponse(
    [property: JsonPropertyName("organization")]
    OrganizationRefResponse Organization,
    [property: JsonPropertyName("key_id")] string KeyId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_on")] DateTimeOffset CreatedOn,
    [property: JsonPropertyName("created_by")] string CreatedBy,
    [property: JsonPropertyName("created_by_name")] string CreatedByName,
    [property: JsonPropertyName("permissions")] string[] Permissions);