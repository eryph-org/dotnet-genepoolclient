using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;


public record GetGeneResponse
{
    [JsonConstructor]
    public GetGeneResponse(GenesetRefResponse? GenesetTag, string Gene, Uri? Uri, GeneManifestData Manifest, 
        bool? Available, DateTimeOffset? DownloadExpires, GenePartDownloadUri[]? DownloadUris, GeneUploadStatusResponse? UploadStatus,
        Uri? ContentUri, GetGeneContentResponse? Content)
    {
        this.GenesetTag = GenesetTag;
        this.Gene = Gene;
        this.Uri = Uri;
        this.Manifest = Manifest;
        this.Available = Available;
        this.DownloadExpires = DownloadUris?.Length > 0 ? DownloadExpires : null;
        this.DownloadUris = DownloadUris;
        this.UploadStatus = UploadStatus;
        this.ContentUri = ContentUri;
        this.Content = Content;
    }

    [JsonPropertyName("geneset_tag")]
    public GenesetRefResponse? GenesetTag { get; init; }


    [JsonPropertyName("gene")]
    public string Gene { get; init; }

    [JsonPropertyName("uri")]
    public Uri? Uri { get; init; }


    [JsonPropertyName("content_uri")]
    public Uri? ContentUri { get; init; }

    [JsonPropertyName("content")]
    public GetGeneContentResponse? Content { get; init; }

    [JsonPropertyName("manifest")]
    public GeneManifestData Manifest { get; init; }

    [JsonPropertyName("available")]
    public bool? Available { get; init; }

    [JsonPropertyName("download_expires")]
    public DateTimeOffset? DownloadExpires { get; init; }


    [JsonPropertyName("download_uris")]
    public GenePartDownloadUri[]? DownloadUris { get; init; }

    [JsonPropertyName("upload_status")]
    public GeneUploadStatusResponse? UploadStatus { get; init; }

}