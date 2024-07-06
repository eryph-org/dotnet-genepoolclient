using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetTagResponse
{
    [JsonConstructor]
    public GenesetTagResponse(string Name, GenesetResponse? Geneset, string Tag, 
        Uri? Uri,
        string? ETag,
        GenesetTagManifestData? Manifest,
        IDictionary<string, string>? Metadata,
        Uri? DownloadUri,
        Uri? GenesUri,
        GetGeneResponse[]? Genes,
        string? GenesContinuationToken,
        Uri? GenesContinuationUri)
    {
        this.Name = Name;
        this.Geneset = Geneset;
        this.Tag = Tag;
        this.Uri = Uri;
        this.ETag = ETag;
        this.Manifest = Manifest;
        this.Metadata = Metadata;
        this.DownloadUri = DownloadUri;
        this.GenesUri = GenesUri;
        this.Genes = Genes;
        this.GenesContinuationToken = GenesContinuationToken;
        this.GenesContinuationUri = GenesContinuationUri;
    }

    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("geneset")]
    public GenesetResponse? Geneset { get; init; }

    [JsonPropertyName("tag")] public string Tag { get; init; }


    [JsonPropertyName("uri")]
    public Uri? Uri { get; init; }

    [JsonPropertyName("etag")]
    public string? ETag { get; init; }


    [JsonPropertyName("manifest")]
    public GenesetTagManifestData? Manifest { get; init; }

    [JsonPropertyName("metadata")]
    public IDictionary<string,string>? Metadata { get; init; }

    [JsonPropertyName("download_uri")]
    public Uri? DownloadUri { get; init; }

    [JsonPropertyName("genes_uri")]
    public Uri? GenesUri { get; init; }

    [JsonPropertyName("genes")] public GetGeneResponse[]? Genes { get; init; }
    [JsonPropertyName("genes_continuation_token")] public string? GenesContinuationToken { get; init; }
    [JsonPropertyName("genes_continuation_uri")] public Uri? GenesContinuationUri { get; init; }

}