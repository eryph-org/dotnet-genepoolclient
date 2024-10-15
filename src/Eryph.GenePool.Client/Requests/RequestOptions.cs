using System;
using Azure;

namespace Eryph.GenePool.Client.Requests;

/// <summary>
/// Base class for all request options
/// </summary>
public class RequestOptions
{
    /// <summary>
    /// Response callback. Will be called after response is received and before message is disposed.
    /// </summary>
    public Action<Response>? OnResponse { get; set; }


}