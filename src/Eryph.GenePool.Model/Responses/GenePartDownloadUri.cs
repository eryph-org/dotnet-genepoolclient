using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenePartDownloadUri
{
    [JsonConstructor]
    public GenePartDownloadUri(string Part, Uri DownloadUri)
    {
        this.Part = Part;
        this.DownloadUri = DownloadUri;
    }

    [JsonPropertyName("part")] public string Part { get; init; }

    [JsonPropertyName("download_uri")]
    public Uri DownloadUri { get; init; }

    public void Deconstruct(out string part, out Uri downloadUri)
    {
        part = Part;
        downloadUri = DownloadUri;
    }
}