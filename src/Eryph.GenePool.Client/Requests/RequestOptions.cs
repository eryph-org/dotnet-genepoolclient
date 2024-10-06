using System;
using Azure;
using Eryph.GenePool.Model.Requests.User;

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

/// <summary>
/// Get User request options
/// </summary>
public class GetUserRequestOptions : RequestOptions
{
    /// <summary>
    /// Expand settings
    /// </summary>
    public ExpandFromUser Expand { get; set; }
}


/// <summary>
/// Base options for list requests
/// </summary>
public class ContinuingListRequestOptions : RequestOptions
{
    /// <summary>
    /// Continuation token for paging
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Hint for page size, backend may return more or less items than requested
    /// </summary>
    public int? PageSizeHint { get; set; }
}

public class ListRecycleBinRequestOptions : ContinuingListRequestOptions
{

}
