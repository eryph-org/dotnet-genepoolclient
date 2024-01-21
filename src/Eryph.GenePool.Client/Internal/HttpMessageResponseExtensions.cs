using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client.Internal;

internal static class HttpMessageResponseExtensions
{
    internal static async ValueTask<TResponse> DeserializeResponseAsync<TResponse>(this HttpMessage message, CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        var messageResponse = message.Response;
        ThrowOnInvalidContent(messageResponse);

        using var document =
            await JsonDocument.ParseAsync(messageResponse.ContentStream!, cancellationToken: cancellationToken).ConfigureAwait(false);

        return ParseDocumentToResponse<TResponse>(messageResponse, document);

    }

    private static TResponse ParseDocumentToResponse<TResponse>(Response messageResponse, JsonDocument document)
        where TResponse : ResponseBase
    {
        if (messageResponse.IsError)
        {
            ErrorResponse? errorResponse = null;

            if (messageResponse.ContentStream != null)
            {
                try
                {
                    errorResponse = document.Deserialize<ErrorResponse>();
                    if (errorResponse is not { ResponseType: ResponseType.Error })
                        throw new Exception();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            errorResponse ??= new ErrorResponse
            {
                StatusCode = (HttpStatusCode)messageResponse.Status,
                StatusCodeString = messageResponse.ReasonPhrase,
                ResponseType = ResponseType.Error,
                ResponseTypeString = "error"
            };

            throw new ErrorResponseException(errorResponse, errorResponse.Message ??
                $"Request failed with status code {messageResponse.Status} {messageResponse.ReasonPhrase}");
        }

        var response = document.Deserialize<TResponse>();
        return response ?? throw new InvalidCastException($"Response could not be mapped to type {typeof(TResponse)}");

    }

    private static void ThrowOnInvalidContent(Response response)
    {
        if (response.ContentStream == null || response.Headers.ContentLength== 0)
        {
            var errorResponse = new ErrorResponse
            {
                StatusCode = (HttpStatusCode)response.Status,
                StatusCodeString = response.ReasonPhrase,
                ResponseType = ResponseType.Error,
                ResponseTypeString = "error"
            };

            throw new ErrorResponseException(errorResponse,
                response.IsError
                    ? $"Request failed with status code {response.Status} {response.ReasonPhrase}"
                    : $"Content missing for successful response.");

        }


    }

    internal static TResponse DeserializeResponse<TResponse>(this HttpMessage message) where TResponse : ResponseBase
    {
        var messageResponse = message.Response;

        ThrowOnInvalidContent(messageResponse);
        using var document = JsonDocument.Parse(messageResponse.ContentStream!);

        return ParseDocumentToResponse<TResponse>(messageResponse, document);

    }
}