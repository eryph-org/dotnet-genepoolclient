using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses;

[PublicAPI]
public record ErrorResponse : ResponseBase
{
    [JsonPropertyName("message")]
    public string? Message { get; init; }

}