using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

public class GenesetManifestData
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("geneset")]
    public string? Geneset { get; set; }

    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("description_markdown")]
    public string? DescriptionMarkdown { get; set; }

    [JsonPropertyName("description_markdown_file")]
    public string? DescriptionMarkdownFile { get; set; }


    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Maps a catlet drive name (e.g. <c>sda</c>) to an ordered list of
    /// candidate cloud images that may be substituted for that drive when
    /// building on a public cloud. Order expresses preference.
    /// </summary>
    [JsonPropertyName("cloud_compatibility")]
    public Dictionary<string, CloudImageReference[]>? CloudCompatibility { get; set; }
}
