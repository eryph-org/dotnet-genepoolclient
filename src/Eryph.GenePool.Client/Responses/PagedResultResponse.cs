using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses
{
    [PublicAPI]
    public record PagedResultResponse<TValue> : ResponseBase, IPagedResultResponse
    {
        public IEnumerable<TValue>? Values { get; set; }

        IEnumerable<object>? IListResultResponse.Values => Values?.Cast<object>();


        public string? ContinuationToken { get; init; }
        public long? Total { get; init; }
        public long? PageSize { get; init; }
    }



}