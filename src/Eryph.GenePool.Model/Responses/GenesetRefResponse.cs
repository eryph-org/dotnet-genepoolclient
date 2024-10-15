using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenesetRefResponse([property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("uri")] Uri? Uri);