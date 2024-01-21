namespace Eryph.GenePool.Client.Responses;

public interface IPagedResultResponse : IListResultResponse
{
    string? ContinuationToken { get; }
    long? Total { get;  }
    long? PageSize { get; }
}