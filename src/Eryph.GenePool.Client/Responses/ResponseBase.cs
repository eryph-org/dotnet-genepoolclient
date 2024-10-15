using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses;

[PublicAPI]
public abstract record ResponseBase : IResponse
{

    [JsonPropertyName("status_code")]
    public HttpStatusCode StatusCode { get; set; }

    [JsonPropertyName("status_code_string")]
    public string? StatusCodeString { get; init; }

    [JsonPropertyName("response_type")]
    public ResponseType ResponseType { get; init; }

    [JsonPropertyName("response_type_string")]
    public string? ResponseTypeString { get; init; }

}