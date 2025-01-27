using System;
using System.Data;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Client.Internal;

internal static class HttpMessageResponseExtensions
{
    internal static async ValueTask<Response<TResponse>> DeserializeResponseAsync<TResponse>(
        this HttpMessage message,
        CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        ThrowOnInvalidContent(message.Response);

        try
        {
            using var document = await JsonDocument.ParseAsync(
                    message.Response.ContentStream!,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            
            return ParseDocumentToResponse<TResponse>(message.Response, document);
        }
        catch (JsonException jex)
        {
            throw new ErrorResponseException(
                CreateInvalidContentResponse(message.Response),
                $"The response does not contain valid JSON: {jex.Message}.",
                jex);
        }
    }

    internal static Response<TResponse> DeserializeResponse<TResponse>(
        this HttpMessage message)
        where TResponse : ResponseBase
    {
        var messageResponse = message.Response;

        ThrowOnInvalidContent(messageResponse);
        try
        {
            using var document = JsonDocument.Parse(messageResponse.ContentStream!);
            return ParseDocumentToResponse<TResponse>(messageResponse, document);
        }
        catch (JsonException jex)
        {
            throw new ErrorResponseException(
                CreateInvalidContentResponse(message.Response),
                $"The response does not contain valid JSON: {jex.Message}.",
                jex);
        }
    }

    private static Response<TResponse> ParseDocumentToResponse<TResponse>(
        Response messageResponse,
        JsonDocument document)
        where TResponse : ResponseBase
    {
        if (messageResponse.IsError)
        {
            var errorResponse = Deserialize<ErrorResponse>(document, messageResponse);
            var message = string.IsNullOrWhiteSpace(errorResponse.Message)
                ? $"The request failed with status code {messageResponse.Status} {messageResponse.ReasonPhrase}"
                : errorResponse.Message;
            throw new ErrorResponseException(errorResponse, message);
        }

        var response = Deserialize<TResponse>(document, messageResponse);
        return new GenepoolResponse<TResponse>(response, messageResponse);
    }

    private static void ThrowOnInvalidContent(Response response)
    {
        var invalidContentResponse = new InvalidContentResponse
        {
            StatusCode = (HttpStatusCode)response.Status,
            StatusCodeString = response.ReasonPhrase,
            ResponseType = ResponseType.InvalidContent,
            ResponseTypeString = "invalid content"
        };

        if (response.ContentStream is null || response.Headers.ContentLength == 0)
        {
            throw new ErrorResponseException(
                invalidContentResponse,
                "The response has no content.");
        }

        if ((response.Headers.ContentType ?? "") != ContentType.ApplicationJson)
        {
            throw new ErrorResponseException(
                invalidContentResponse,
                $"The content type of the response is invalid: {response.Headers.ContentType}.");
        }
    }

    private static T Deserialize<T>(JsonDocument document, Response response)
    {
        try
        {
            var result = document.Deserialize<T>(GeneModelDefaults.SerializerOptions);
            if (result is null)
                throw new ErrorResponseException(
                    CreateInvalidContentResponse(response),
                    "The JSON response is null.");

            return result;
        }
        catch (JsonException jex)
        {
            throw new ErrorResponseException(
                CreateInvalidContentResponse(response),
                $"The JSON response is not a valid {typeof(T).Name}: {jex.Message}.",
                jex);
        }
    }

    private static InvalidContentResponse CreateInvalidContentResponse(
        Response response) =>
        new()
        {
            StatusCode = (HttpStatusCode)response.Status,
            StatusCodeString = response.ReasonPhrase,
            ResponseType = ResponseType.InvalidContent,
            ResponseTypeString = "invalid content"
        };
}
