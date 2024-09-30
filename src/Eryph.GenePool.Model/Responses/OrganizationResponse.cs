using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record OrganizationResponse(
    [property: JsonPropertyName("id")] Guid Id, 
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("identity_org_id")] Guid? IdentityOrgId,
    [property: JsonPropertyName("etag")] string? ETag);