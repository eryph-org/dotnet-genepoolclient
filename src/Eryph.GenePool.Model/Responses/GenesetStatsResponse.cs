using System;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

/// <summary>
/// Response for geneset statistics
/// </summary>
public record GenesetStatsResponse
{
    [JsonConstructor]
    public GenesetStatsResponse(GenesetRefResponse Geneset,
        GenesetStatsStatus Status,
        long? Downloads,
        long? TotalSize,
        long? Size)
    {
        this.Geneset = Geneset;
        this.Status = Status;
        this.Downloads = Downloads;
        this.TotalSize = TotalSize;
        this.Size = Size;
    }

    [JsonPropertyName("geneset")]
    public GenesetRefResponse Geneset { get; init; }


    [JsonPropertyName("status")]
    public GenesetStatsStatus Status { get; init; }

    [JsonPropertyName("status_string")] public string StatusString => Status.ToString().ToLowerInvariant();

    [JsonPropertyName("downloads")]
    public long? Downloads { get; init; }

    [JsonPropertyName("total_size")]
    public long? TotalSize { get; init; }


    [JsonPropertyName("size")]
    public long? Size { get; init; }


}