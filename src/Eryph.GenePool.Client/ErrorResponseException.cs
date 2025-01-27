using System;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client;

public class ErrorResponseException : Exception
{
    public ResponseBase Response { get; }

    public ErrorResponseException(ResponseBase response)
    {
        Response = response;
    }

    public ErrorResponseException(ResponseBase response, string message) : base(message)
    {
        Response = response;
    }

    public ErrorResponseException(ResponseBase response, string message, Exception inner) : base(message, inner)
    {
        Response = response;
    }
}
