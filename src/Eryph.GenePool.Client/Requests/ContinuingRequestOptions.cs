namespace Eryph.GenePool.Client.Requests;

/// <summary>
/// Base options for list requests
/// </summary>
public class ContinuingRequestOptions<TOption> where TOption: RequestOptions, new()
{
    /// <summary>
    /// Continuation token for paging
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Hint for page size, backend may return more or less items than requested
    /// </summary>
    public int? PageSizeHint { get; set; }

    public TOption RequestOptions { get; set; } = new TOption();
}