using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Eryph.GenePool.Client;

/// <summary>
/// A cache for Tokens.
/// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
// SemaphoreSlim only needs to be disposed when AvailableWaitHandle is called.
internal class TokenCache
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    internal Func<IPublicClientApplication> PublicClientApplicationFactory;
    private readonly bool _allowUnencryptedStorage;
    private readonly string? _name;
    private readonly AsyncLockWithValue<MsalCacheHelperWrapper> _cacheHelperLock = new();
    private readonly MsalCacheHelperWrapper _cacheHelperWrapper;

    /// <summary>
    /// The internal state of the cache.
    /// </summary>
    internal byte[] Data { get; private set; }

    /// <summary>
    /// Creates a new instance of <see cref="TokenCache"/> with the specified options.
    /// </summary>
    /// <param name="options">Options controlling the storage of the <see cref="TokenCache"/>.</param>
    public TokenCache(TokenCachePersistenceOptions? options = null)
        : this(options, default)
    { }

    internal TokenCache(TokenCachePersistenceOptions? options, MsalCacheHelperWrapper? cacheHelperWrapper, 
        Func<IPublicClientApplication>? publicApplicationFactory = null)
    {
        _cacheHelperWrapper = cacheHelperWrapper ?? new MsalCacheHelperWrapper();
        PublicClientApplicationFactory = publicApplicationFactory ?? new Func<IPublicClientApplication>(() => PublicClientApplicationBuilder.Create(Guid.NewGuid().ToString()).Build());
        Data = Array.Empty<byte>();
        _allowUnencryptedStorage = options?.UnsafeAllowUnencryptedStorage ?? false;
        _name = options?.Name ?? Constants.DefaultEryphTokenCacheName;

    }

    internal virtual async Task RegisterCache(bool async, ITokenCache tokenCache, CancellationToken cancellationToken)
    {
        var cacheHelper = await GetCacheHelperAsync(async, cancellationToken).ConfigureAwait(false);
        cacheHelper.RegisterCache(tokenCache);

    }


    private async Task<MsalCacheHelperWrapper> GetCacheHelperAsync(bool async, CancellationToken cancellationToken)
    {
        using var asyncLock = await _cacheHelperLock.GetLockOrValueAsync(async, cancellationToken).ConfigureAwait(false);

        if (asyncLock.HasValue)
        {
            return asyncLock.Value;
        }

        MsalCacheHelperWrapper cacheHelper;

        try
        {
            cacheHelper = await GetProtectedCacheHelperAsync(async, _name).ConfigureAwait(false);

            cacheHelper.VerifyPersistence();
        }
        catch (MsalCachePersistenceException)
        {
            if (_allowUnencryptedStorage)
            {
                cacheHelper = await GetFallbackCacheHelperAsync(async, _name).ConfigureAwait(false);

                cacheHelper.VerifyPersistence();
            }
            else
            {
                throw;
            }
        }

        asyncLock.SetValue(cacheHelper);

        return cacheHelper;
    }

    private async Task<MsalCacheHelperWrapper> GetProtectedCacheHelperAsync(bool async, string? name)
    {
        var storageProperties = new StorageCreationPropertiesBuilder(name, Constants.DefaultMsalTokenCacheDirectory)
            .WithMacKeyChain(Constants.DefaultMsalTokenCacheKeychainService, name)
            .WithLinuxKeyring(Constants.DefaultMsalTokenCacheKeyringSchema, Constants.DefaultMsalTokenCacheKeyringCollection, name, Constants.DefaultMsalTokenCacheKeyringAttribute1, Constants.DefaultMsalTokenCacheKeyringAttribute2)
            .Build();

        var cacheHelper = await InitializeCacheHelper(async, storageProperties).ConfigureAwait(false);

        return cacheHelper;
    }

    private async Task<MsalCacheHelperWrapper> GetFallbackCacheHelperAsync(bool async, string? name = 
        Constants.DefaultEryphTokenCacheName)
    {
        var storageProperties = new StorageCreationPropertiesBuilder(name, 
                Constants.DefaultMsalTokenCacheDirectory)
            .WithMacKeyChain(Constants.DefaultMsalTokenCacheKeychainService, name)
            .WithLinuxUnprotectedFile()
            .Build();

        var cacheHelper = await InitializeCacheHelper(async, storageProperties).ConfigureAwait(false);

        return cacheHelper;
    }

    private async Task<MsalCacheHelperWrapper> InitializeCacheHelper(bool async, StorageCreationProperties storageProperties)
    {
        if (async)
        {
            await _cacheHelperWrapper.InitializeAsync(storageProperties).ConfigureAwait(false);
        }
        else
        {
            _cacheHelperWrapper.InitializeAsync(storageProperties).GetAwaiter().GetResult();
        }
        return _cacheHelperWrapper;
    }

}