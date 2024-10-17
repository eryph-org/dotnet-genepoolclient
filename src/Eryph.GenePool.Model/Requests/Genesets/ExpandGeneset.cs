using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genesets;

public struct ExpandGeneset
{
    [JsonPropertyName("metadata")]
    public bool Metadata { get; set; }

    [JsonPropertyName("description")]
    public bool Description { get; set; }
    
    [JsonPropertyName("tags")]
    public ExpandTagsFromGeneset? Tags { get; set; }

}