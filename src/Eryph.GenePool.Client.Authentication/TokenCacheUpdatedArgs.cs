using System;

namespace Eryph.GenePool.Client;

/// <summary>
/// Data regarding an update of a token cache.
/// </summary>
public class TokenCacheUpdatedArgs
{
    internal TokenCacheUpdatedArgs(ReadOnlyMemory<byte> cacheData)
    {
        UnsafeCacheData = cacheData;
    }

    /// <summary>
    /// The <see cref="TokenCachePersistenceOptions"/> instance which was updated.
    /// </summary>
    public ReadOnlyMemory<byte> UnsafeCacheData { get; }


}