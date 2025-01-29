using System;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
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
            throw CreateInvalidContentException(
                message.Response,
                $"The response does not contain valid JSON: {jex.Message}",
                jex);
        }
    }

    internal static Response<TResponse> DeserializeResponse<TResponse>(
        this HttpMessage message)
        where TResponse : ResponseBase
    {
        ThrowOnInvalidContent(message.Response);
        
        try
        {
            using var document = JsonDocument.Parse(message.Response.ContentStream!);
            return ParseDocumentToResponse<TResponse>(message.Response, document);
        }
        catch (JsonException jex)
        {
            throw CreateInvalidContentException(
                message.Response,
                $"The response does not contain valid JSON: {jex.Message}",
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
            throw new ErrorResponseException(errorResponse, message, (HttpStatusCode)messageResponse.Status);
        }

        var response = Deserialize<TResponse>(document, messageResponse);
        return new GenepoolResponse<TResponse>(response, messageResponse);
    }

    private static void ThrowOnInvalidContent(Response response)
    {
        if (response.ContentStream is null || response.Headers.ContentLength == 0)
        {
            throw CreateInvalidContentException(
                response,
                "The response has no content");
        }

        if (!MediaTypeHeaderValue.TryParse(response.Headers.ContentType, out var headerValue))
        {
            throw CreateInvalidContentException(
                response,
                $"The content type of the response is invalid: {response.Headers.ContentType}");
        }

        if (string.Equals(headerValue.MediaType, "text/html", StringComparison.OrdinalIgnoreCase))
        {
            throw CreateInvalidContentException(
                response,
                "The response is an HTML page. Please verify that your internet access"
                + " is not blocked by a proxy or capture portal.");
        }

        if (!string.Equals(headerValue.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw CreateInvalidContentException(
                response,
                $"The content type of the response is invalid: {response.Headers.ContentType}");
        }
    }

    private static T Deserialize<T>(JsonDocument document, Response response)
    {
        try
        {
            var result = document.Deserialize<T>(GeneModelDefaults.SerializerOptions);
            if (result is null)
                throw CreateInvalidContentException(
                    response,
                    "The JSON response is null");

            return result;
        }
        catch (JsonException jex)
        {
            throw CreateInvalidContentException(
                response,
                $"The JSON response is not a valid {typeof(T).Name}: {jex.Message}",
                jex);
        }
    }

    private static GenepoolClientException CreateInvalidContentException(
        Response response,
        string message) =>
        new(message, (HttpStatusCode)response.Status);

    private static GenepoolClientException CreateInvalidContentException(
        Response response,
        string message,
        Exception innerException) =>
        new(message, (HttpStatusCode)response.Status, innerException);
}
