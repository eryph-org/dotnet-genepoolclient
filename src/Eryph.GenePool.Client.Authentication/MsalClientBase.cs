using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

internal abstract class MsalClientBase<TClient>(
    string authorityUri,
    string? clientId,
    TokenCachePersistenceOptions? cacheOptions)
    where TClient : IClientApplicationBase
{
    private readonly AsyncLockWithValue<(TClient Client, TokenCache Cache)> _clientAsyncLock = new();

    internal string AuthorityUri { get; } = authorityUri;
    internal string? ClientId { get; } = clientId;

    protected abstract ValueTask<TClient> CreateClientAsync(bool async, CancellationToken cancellationToken);

    protected async ValueTask<TClient> GetClientAsync(bool async, CancellationToken cancellationToken)
    {
        using var asyncLock = await _clientAsyncLock.GetLockOrValueAsync(async, cancellationToken).ConfigureAwait(false);

        if (asyncLock.HasValue)
        {
            return asyncLock.Value.Client;
        }

        var client = await CreateClientAsync(async, cancellationToken).ConfigureAwait(false);

        TokenCache? tokenCache = null;
        if (cacheOptions != null)
        {
            tokenCache = new TokenCache(cacheOptions);
            await tokenCache.RegisterCache(async, client.UserTokenCache, cancellationToken).ConfigureAwait(false);

            if (client is IConfidentialClientApplication cca)
            {
                await tokenCache.RegisterCache(async, cca.AppTokenCache, cancellationToken).ConfigureAwait(false);
            }
        }

        asyncLock.SetValue((Client: client, Cache: tokenCache)!);
        return client;
    }

}