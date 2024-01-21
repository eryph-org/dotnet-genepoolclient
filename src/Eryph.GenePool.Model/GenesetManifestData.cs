using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

public class GenesetManifestData
{
    [JsonPropertyName("geneset")]
    public string? Geneset { get; set; }

    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("description_markdown")]
    public string? DescriptionMarkdown { get; set; }

    [JsonPropertyName("description_markdown_file")]
    public string? DescriptionMarkdownFile { get; set; }


    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}