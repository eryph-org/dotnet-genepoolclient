using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

public class GenesetTagManifestData
{
    [JsonPropertyName("geneset")]
    public string? Geneset { get; set; }

    [JsonPropertyName("ref")]
    public string? Reference { get; set; }

    [JsonPropertyName("volumes")]
    public GeneReferenceData[]? VolumeGenes { get; set; }

    [JsonPropertyName("fodder")]
    public GeneReferenceData[]? FodderGenes { get; set; }

    [JsonPropertyName("catlet")]
    public string? CatletGene { get; set; }

    [JsonPropertyName("parent")]

    public string? Parent { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}