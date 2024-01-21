using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GetGeneDownloadResponse
{
    [JsonConstructor]
    public GetGeneDownloadResponse(string Gene, GeneManifestData Manifest, GenePartDownloadUri[]? DownloadUris)
    {
        this.Gene = Gene;
        this.Manifest = Manifest;
        this.DownloadUris = DownloadUris;
    }


    [JsonPropertyName("gene")]
    public string Gene { get; init; }

    [JsonPropertyName("manifest")]
    public GeneManifestData Manifest { get; init; }

    [JsonPropertyName("download_uris")]
    public GenePartDownloadUri[]? DownloadUris { get; init; }

    public void Deconstruct(out string gene, out GeneManifestData manifest, out GenePartDownloadUri[]? downloadUris)
    {
        gene = Gene;
        manifest = Manifest;
        downloadUris = DownloadUris;
    }
}