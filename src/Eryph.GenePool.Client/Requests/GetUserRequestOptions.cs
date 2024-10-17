using Eryph.GenePool.Model.Requests.User;

namespace Eryph.GenePool.Client.Requests;

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