using System;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client;

public class ErrorResponseException : Exception
{
    public ErrorResponse Response { get; }

    public ErrorResponseException(ErrorResponse response)
    {
        Response = response;
    }

    public ErrorResponseException(ErrorResponse response, string message) : base(message)
    {
        Response = response;
    }

    public ErrorResponseException(ErrorResponse response, string message, Exception inner) : base(message, inner)
    {
        Response = response;
    }
}