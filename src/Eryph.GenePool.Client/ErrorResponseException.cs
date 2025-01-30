using System;
using System.Net;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client;

/// <summary>
/// This exception is thrown when a request to the Genepool API
/// returned an error response.
/// </summary>
public class ErrorResponseException : GenepoolClientException
{
    /// <summary>
    /// The received error response.
    /// </summary>
    public ErrorResponse Response { get; }

    public ErrorResponseException(
        ErrorResponse response,
        string message,
        HttpStatusCode statusCode)
        : base(message, statusCode)
    {
        Response = response;
    }
}
