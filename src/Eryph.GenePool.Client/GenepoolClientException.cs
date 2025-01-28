using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Eryph.GenePool.Client;

/// <summary>
/// This exception is thrown when a request to the Genepool API
/// has not been successful.
/// </summary>
public class GenepoolClientException : Exception
{
    public GenepoolClientException(string message)
        : base(message)
    {
    }

    public GenepoolClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GenepoolClientException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public GenepoolClientException(string message, HttpStatusCode statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// The status code returned by the server. Can be <see langword="null"/>
    /// if no response has been received.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }
}
