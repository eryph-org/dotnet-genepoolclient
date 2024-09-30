using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record IdentityOrganizationResponse(
    [property: JsonPropertyName("id")] Guid? Id, 
    [property: JsonPropertyName("name")] string? Name, 
    [property: JsonPropertyName("roles")] string[]? Roles,
    [property: JsonPropertyName("customer_no")] string? CustomerNo);