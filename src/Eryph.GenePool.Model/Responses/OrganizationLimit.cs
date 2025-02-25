using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record OrganizationLimit(

    [property: JsonPropertyName("public_genesets")] int PublicGenesets,
    [property: JsonPropertyName("private_genesets")] int PrivateGeneSets,
    [property: JsonPropertyName("public_geneset_size")] long PublicGenesetsSize,
    [property: JsonPropertyName("private_geneset_size")] long PrivateGenesetsSize,
    [property: JsonPropertyName("apikeys")] int ApiKeys);