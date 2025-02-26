using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record UserOrganizationResponse(
    [property: JsonPropertyName("org")] OrganizationRefResponse Org,
    [property: JsonPropertyName("is_owner")] bool? IsOwner,
    [property: JsonPropertyName("permissions")] string[] Permissions);