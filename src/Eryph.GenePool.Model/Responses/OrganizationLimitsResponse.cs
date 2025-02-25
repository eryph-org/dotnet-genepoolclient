using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record OrganizationLimitsResponse(
    [property: JsonPropertyName("org")] OrganizationRefResponse Org,
    [property: JsonPropertyName("limits")] OrganizationLimit Limits,
    [property: JsonPropertyName("limits_usage")] OrganizationLimit LimitsUsage,
    [property: JsonPropertyName("policy")] string Policy
    );


[method: JsonConstructor]
public record OrganizationUsageResponse(
    [property: JsonPropertyName("period")] long Period,
    [property: JsonPropertyName("aggregate")] string Aggregate,
    [property: JsonPropertyName("predicted")] bool Predicted,
    [property: JsonPropertyName("public_genesets_size")] long? PublicGenesetsSize,
    [property: JsonPropertyName("private_genesets_size")] long? PrivateGenesetsSize,
    [property: JsonPropertyName("apikeys")] int? ApiKeyCount,
    [property: JsonPropertyName("public_genesets_size_billable")] long? PublicGenesetsSizeBillable,
    [property: JsonPropertyName("private_genesets_size_billable")] long? PrivateGenesetsSizeBillable,
    [property: JsonPropertyName("public_genesets_size_total")] long? PublicGenesetsSizeTotal,
    [property: JsonPropertyName("private_genesets_size_total")] long? PrivateGenesetsSizeTotal,
    [property: JsonPropertyName("apikeys_billable")] int? ApiKeyCountBillable
);