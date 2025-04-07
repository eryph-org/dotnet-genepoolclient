using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

/// <summary>
/// Search keys for direct access to search API.
/// </summary>
/// <param name="Genesets"></param>
[method: JsonConstructor]
public record GetSearchKeysResponse(
    [property: JsonPropertyName("genesets")] string Genesets);