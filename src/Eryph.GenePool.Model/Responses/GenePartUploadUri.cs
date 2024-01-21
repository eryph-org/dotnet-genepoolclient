using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenePartUploadUri
{
    [JsonConstructor]
    public GenePartUploadUri(string Part, Uri UploadUri, DateTimeOffset Expires)
    {
        this.Part = Part;
        this.UploadUri = UploadUri;
        this.Expires = Expires;
    }

    [JsonPropertyName("part")]
    public string Part { get; init; }

    [JsonPropertyName("upload_uri")]
    public Uri UploadUri { get; init; }

    [JsonPropertyName("expires")]
    public DateTimeOffset Expires { get; init; }

    public void Deconstruct(out string part, out Uri uploadUri, out DateTimeOffset expires)
    {
        part = Part;
        uploadUri = UploadUri;
        expires = Expires;
    }
}