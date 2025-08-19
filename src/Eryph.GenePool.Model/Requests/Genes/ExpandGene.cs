using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genes;

public struct ExpandGene
{
    [JsonPropertyName("content")]
    public bool Content { get; set; }
}
