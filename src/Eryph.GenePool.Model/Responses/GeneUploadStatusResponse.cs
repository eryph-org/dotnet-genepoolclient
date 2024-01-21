using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GeneUploadStatusResponse
{
    [JsonConstructor]
    public GeneUploadStatusResponse(long UploadedSize, string[] UploadedParts, string[] ConfirmedParts)
    {
        this.UploadedSize = UploadedSize;
        this.UploadedParts = UploadedParts;
        this.ConfirmedParts = ConfirmedParts;
    }

    [JsonPropertyName("uploaded_size")]
    public long UploadedSize { get; init; }

    [JsonPropertyName("uploaded_parts")]
    public string[] UploadedParts { get; init; }

    [JsonPropertyName("confirmed_parts")]
    public string[] ConfirmedParts { get; init; }


}