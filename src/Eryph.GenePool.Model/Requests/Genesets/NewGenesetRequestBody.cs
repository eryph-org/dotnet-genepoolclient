using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genesets;

public class NewGenesetRequestBody
{
    [JsonPropertyName("geneset")]
    public string? Geneset { get; set; }

    /// <summary>
    /// The geneset manifest version the client understands. The server respects
    /// this when saving the manifest, so a client never writes fields from a
    /// newer manifest version than it declares here.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("short_description")]

    public string? ShortDescription { get; set; }

    [JsonPropertyName("description")]

    public string? Description { get; set; }

    [JsonPropertyName("description_markdown")]

    public string? DescriptionMarkdown { get; set; }


    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    [JsonPropertyName("metadata")]
    public IDictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("cloud_compatibility")]
    public IDictionary<string, CloudImageReference[]>? CloudCompatibility { get; set; }
}