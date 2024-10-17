using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

internal class MsalConfidentialClient(
    string authorityUri,
    string? clientId,
    string clientSecret,
    string? redirectUrl,
    TokenCredentialOptions? options)
    : MsalClientBase<IConfidentialClientApplication>(authorityUri, clientId, options?.TokenCachePersistenceOptions)
{
    internal readonly string ClientSecret = clientSecret;

    internal string? RedirectUrl { get; } = redirectUrl;

    protected override ValueTask<IConfidentialClientApplication> CreateClientAsync(bool async, CancellationToken cancellationToken)
    {
        return CreateClientCoreAsync(async, cancellationToken);
    }

    protected virtual ValueTask<IConfidentialClientApplication> CreateClientCoreAsync(bool async, CancellationToken cancellationToken)
    {

        var confClientBuilder = ConfidentialClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(AuthorityUri);

        if (!string.IsNullOrEmpty(RedirectUrl))
        {
            confClientBuilder.WithRedirectUri(RedirectUrl);
        }

        return new ValueTask<IConfidentialClientApplication>(confClientBuilder.Build());
    }

    public virtual async ValueTask<AuthenticationResult> AcquireTokenForClientAsync(
        string[] scopes,
        bool async,
        CancellationToken cancellationToken)
    {
        var result = await AcquireTokenForClientCoreAsync(scopes, async, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public virtual async ValueTask<AuthenticationResult> AcquireTokenForClientCoreAsync(
        string[] scopes,
        bool async,
        CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(async, cancellationToken).ConfigureAwait(false);

        var builder = client
            .AcquireTokenForClient(scopes);

        return await builder
            .ExecuteAsync(async, cancellationToken)
            .ConfigureAwait(false);
    }



}