using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

[method: JsonConstructor]
public record GenePartUploadUri([property: JsonPropertyName("part")] string Part,
    [property: JsonPropertyName("upload_uri")]
    Uri UploadUri,
    [property: JsonPropertyName("expires")]
    DateTimeOffset Expires);