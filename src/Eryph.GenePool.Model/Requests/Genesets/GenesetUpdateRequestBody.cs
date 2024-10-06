using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genesets;

public class GenesetUpdateRequestBody
{

    [JsonPropertyName("short_description")]

    public string? ShortDescription { get; set; }

    [JsonPropertyName("description")]

    public string? Description { get; set; }

    [JsonPropertyName("description_markdown")]

    public string? DescriptionMarkdown { get; set; }


    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    [JsonPropertyName("metadata")]
    public IDictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("etag")]
    public string? ETag { get; set; }

}