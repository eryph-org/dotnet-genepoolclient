using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenePartDownloadUri([property: JsonPropertyName("part")] string Part,
    [property: JsonPropertyName("download_uri")]
    Uri DownloadUri)
{
    public void Deconstruct(out string part, out Uri downloadUri)
    {
        part = Part;
        downloadUri = DownloadUri;
    }
}