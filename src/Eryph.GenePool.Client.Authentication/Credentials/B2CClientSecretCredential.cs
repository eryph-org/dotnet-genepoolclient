using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Eryph.GenePool.Client.Internal;

namespace Eryph.GenePool.Client.Credentials;

public class B2CClientSecretCredential : TokenCredential
{
    internal MsalConfidentialClient Client { get; }

    /// <summary>
    /// Gets the client (application) ID of the service principal
    /// </summary>
    internal string? ClientId { get; }

    /// <summary>
    /// Gets the client secret that was generated for the App Registration used to authenticate the client.
    /// </summary>
    internal string? ClientSecret { get; }

    public B2CClientSecretCredential(string authorityUri, string? clientId, string clientSecret)
        : this(authorityUri, clientId, clientSecret, null, null)
    {
    }

    public B2CClientSecretCredential(string tenantId, string? clientId, string clientSecret, B2CClientSecretCredentialOptions options)
        : this(tenantId, clientId, clientSecret, options, null)
    {
    }

    public B2CClientSecretCredential(string authorityUri, string? clientId, string clientSecret, TokenCredentialOptions options)
        : this(authorityUri, clientId, clientSecret, options, null)
    {
    }

    internal B2CClientSecretCredential(string authorityUri, string? clientId, string clientSecret,
        TokenCredentialOptions? options, MsalConfidentialClient? client)
    {
        Argument.AssertNotNull(clientId, nameof(clientId));
        Argument.AssertNotNull(clientSecret, nameof(clientSecret));
        Argument.AssertNotNull(authorityUri, nameof(authorityUri));

        ClientId = clientId;
        ClientSecret = clientSecret;
        Client = client ??
                 new MsalConfidentialClient(
                     authorityUri,
                     clientId,
                     clientSecret,
                     null,
                     options);

    }

    /// <summary>
    /// Obtains a token from Microsoft Entra ID, using the specified client secret to authenticate. Acquired tokens are cached by the credential instance. Token lifetime and refreshing is handled automatically. Where possible, reuse credential instances to optimize cache effectiveness.
    /// </summary>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls.</returns>
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {

        var result = await Client.AcquireTokenForClientAsync(requestContext.Scopes, true, cancellationToken).ConfigureAwait(false);

        return new AccessToken(result.AccessToken, result.ExpiresOn);

    }

    /// <summary>
    /// Obtains a token from Microsoft Entra ID, using the specified client secret to authenticate. Acquired tokens are cached by the credential instance. Token lifetime and refreshing is handled automatically. Where possible, reuse credential instances to optimize cache effectiveness.
    /// </summary>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls.</returns>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        var result = Client.AcquireTokenForClientAsync(requestContext.Scopes, false, cancellationToken).EnsureCompleted();

        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }
}