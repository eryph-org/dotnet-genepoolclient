using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses;

[PublicAPI]
public record SingleResultResponse<TValue> : ResponseBase, ISingleResultResponse
{
    [JsonPropertyName("value")]
    public TValue? Value { get; init; }

    object? ISingleResultResponse.Value => Value;
}