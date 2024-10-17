using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genesets;

public struct ExpandTagsFromGeneset
{
    [JsonPropertyName("metadata")]
    public bool Metadata { get; set; }

    [JsonPropertyName("manifest")]
    public bool Manifest { get; set; }

}