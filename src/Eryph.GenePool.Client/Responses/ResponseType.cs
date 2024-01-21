using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses
{
    [PublicAPI]
    public enum ResponseType
    {
        Error,
        NoResult,
        SingleResult,
        ListResult,
        PagedResult

    }
}