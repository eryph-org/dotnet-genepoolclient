using Eryph.GenePool.Model;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests;

public class NewGeneRequestBody
{
    [JsonPropertyName("geneset")]

    public string? Geneset { get; set; }

    [JsonPropertyName("manifest")]

    public GeneManifestData? Manifest { get; set; }
}