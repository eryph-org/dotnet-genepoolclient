using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genesets;

public struct ExpandTagsFromGeneset
{
    [JsonPropertyName("manifest")]
    public bool Manifest { get; set; }
}
