using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests;

public class ConfirmGeneUploadRequestBody
{
    [JsonPropertyName("part")]
    public string? Part { get; set; }

}