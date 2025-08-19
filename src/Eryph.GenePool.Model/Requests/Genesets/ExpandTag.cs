using System.Text.Json.Serialization;
using Eryph.GenePool.Model.Requests.Genes;

namespace Eryph.GenePool.Model.Requests.Genesets;

public struct ExpandTag
{
    [JsonPropertyName("manifest")]
    public bool Manifest { get; set; }

    [JsonPropertyName("geneset")]
    public ExpandGenesetFromTag? Geneset { get; set; }

    [JsonPropertyName("genes")]
    public ExpandGeneFromTag? Genes { get; set; }
}
