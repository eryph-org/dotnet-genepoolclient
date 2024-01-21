using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests;

public class NewGenesetRequestBody
{
    [JsonPropertyName("geneset")]
    public string? Geneset { get; set; }

    [JsonPropertyName("short_description")]

    public string? ShortDescription { get; set; }

    [JsonPropertyName("description_markdown")]

    public string? DescriptionMarkdown { get; set; }


    [JsonPropertyName("public")]
    public bool? Public { get; set; }
}