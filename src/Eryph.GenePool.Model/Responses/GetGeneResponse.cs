using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GetGeneResponse
{
    [JsonConstructor]
    public GetGeneResponse(GenesetRefResponse Geneset, string Gene, GeneManifestData Manifest, 
        bool Available, DateTimeOffset DownloadExpires, GenePartDownloadUri[]? DownloadUris, GeneUploadStatusResponse? UploadStatus)
    {
        this.Geneset = Geneset;
        this.Gene = Gene;
        this.Manifest = Manifest;
        this.Available = Available;
        this.DownloadExpires = DownloadExpires;
        this.DownloadUris = DownloadUris;
        this.UploadStatus = UploadStatus;
    }

    [JsonPropertyName("geneset")]
    public GenesetRefResponse Geneset { get; init; }

    [JsonPropertyName("gene")]
    public string Gene { get; init; }

    [JsonPropertyName("manifest")]
    public GeneManifestData Manifest { get; init; }

    [JsonPropertyName("available")]
    public bool Available { get; init; }

    [JsonPropertyName("download_expires")]
    public DateTimeOffset DownloadExpires { get; init; }


    [JsonPropertyName("download_uris")]
    public GenePartDownloadUri[]? DownloadUris { get; init; }

    [JsonPropertyName("upload_status")]
    public GeneUploadStatusResponse? UploadStatus { get; init; }

    public void Deconstruct(out GenesetRefResponse geneset, out string gene, 
        out GeneManifestData manifest, out bool available, 
        out DateTimeOffset downloadExpires,
        out GenePartDownloadUri[]? downloadUris, out GeneUploadStatusResponse? uploadStatus)
    {
        geneset = Geneset;
        gene = Gene;
        manifest = Manifest;
        available = Available;
        downloadExpires = DownloadExpires;
        downloadUris = DownloadUris;
        uploadStatus = UploadStatus;
    }
}