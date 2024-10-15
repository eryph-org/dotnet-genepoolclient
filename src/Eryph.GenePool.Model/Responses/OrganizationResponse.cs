using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record OrganizationResponse(
    [property: JsonPropertyName("id")] Guid Id, 
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("identity_org_id")] Guid? IdentityOrgId,
    [property: JsonPropertyName("created_at")] DateTimeOffset? Created,
    [property: JsonPropertyName("genesets_uri")] Uri? GenesetsUri,
    [property: JsonPropertyName("apikeys_uri")] Uri? ApiKeysUri,
    [property: JsonPropertyName("etag")] string? ETag);