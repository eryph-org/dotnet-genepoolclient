using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses;

[PublicAPI]
public record ListResultResponse<TValue> : ResponseBase, IListResultResponse
{
    public IEnumerable<TValue>? Values { get; init; }

    IEnumerable<object>? IListResultResponse.Values => Values?.Cast<object>();
}