using Eryph.GenePool.Model;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests;

public class NewGeneRequestBody
{
    [JsonPropertyName("geneset")]

    public string? Geneset { get; set; }

    [JsonPropertyName("gene")]

    public string? Gene { get; set; }


    [JsonPropertyName("manifest")]

    public GeneManifestData? Manifest { get; set; }

    [JsonPropertyName("yaml_content")]
    public string? YamlContent { get; set; }


}