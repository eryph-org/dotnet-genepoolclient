using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Genes;

public class ConfirmGeneUploadRequestBody
{
    [JsonPropertyName("part")]
    public string? Part { get; set; }

}