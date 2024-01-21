using System.Collections.Generic;

namespace Eryph.GenePool.Client.Responses;

public interface IListResultResponse
{
    IEnumerable<object>? Values { get; }
}