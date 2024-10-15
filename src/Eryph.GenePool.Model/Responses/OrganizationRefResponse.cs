using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record OrganizationRefResponse([property: JsonPropertyName("id")] Guid Id, 
    [property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("uri")] Uri Uri);