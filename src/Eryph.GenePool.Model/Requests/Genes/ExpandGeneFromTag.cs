using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genes;

public struct ExpandGeneFromTag
{
    [JsonPropertyName("manifest")]
    public bool Manifest { get; set; }

    [JsonPropertyName("download_uris")]
    public bool DownloadUris { get; set; }
}
