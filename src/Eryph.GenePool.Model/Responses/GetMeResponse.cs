using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GetMeResponse(
    [property: JsonPropertyName("userid")] string Id,
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("scopes")] string[] Scopes,
    [property: JsonPropertyName("genepool_orgs")] string[]? GenepoolOrgs,
    [property: JsonPropertyName("identity_orgs")] IdentityOrganizationResponse[]? IdentityOrgs);