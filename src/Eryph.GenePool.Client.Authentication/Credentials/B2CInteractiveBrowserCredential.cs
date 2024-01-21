using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Eryph.GenePool.Client.Internal;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client.Credentials;

/// <summary>
/// A <see cref="TokenCredential"/> implementation which launches the system default browser to interactively authenticate a user, and obtain an access token.
/// The browser will only be launched to authenticate the user once, then will silently acquire access tokens through the users refresh token as long as it's valid.
/// </summary>
public class B2CInteractiveBrowserCredential : TokenCredential
{
    internal string? ClientId { get; }
    internal string? LoginHint { get; }
    internal BrowserCustomizationOptions? BrowserCustomization { get; }
    internal MsalPublicClient Client { get; }
    internal bool DisableAutomaticAuthentication { get; }
    internal AuthenticationRecord? Record { get; private set; }

    private const string AuthenticationRequiredMessage = "Interactive authentication is needed to acquire token. Call Authenticate to interactively authenticate.";

    /// <summary>
    /// Creates a new <see cref="B2CInteractiveBrowserCredential"/> with the specified options, which will authenticate users with the specified application.
    /// </summary>
    /// <param name="options">The client options for the newly created <see cref="B2CInteractiveBrowserCredential"/>.</param>
    public B2CInteractiveBrowserCredential(B2CInteractiveBrowserCredentialOptions options)
        : this(options.AuthorityUri, options.ClientId, options, null)
    {
        DisableAutomaticAuthentication = options.DisableAutomaticAuthentication;
        Record = options.AuthenticationRecord;
    }


    internal B2CInteractiveBrowserCredential(string? authorityUri, string? clientId,
        TokenCredentialOptions? options, MsalPublicClient? client)
    {
        Argument.AssertNotNullOrWhiteSpace(clientId, nameof(clientId));
        Argument.AssertNotNullOrWhiteSpace(authorityUri, nameof(authorityUri));
        Debug.Assert(authorityUri != null, nameof(authorityUri) + " != null");

        ClientId = clientId;
        LoginHint = (options as B2CInteractiveBrowserCredentialOptions)?.LoginHint;
        var redirectUrl = (options as B2CInteractiveBrowserCredentialOptions)?.RedirectUri?.AbsoluteUri ?? Constants.DefaultRedirectUrl;
        Client = client ?? new MsalPublicClient(authorityUri, clientId, redirectUrl, options);
        Record = (options as B2CInteractiveBrowserCredentialOptions)?.AuthenticationRecord;
        BrowserCustomization = (options as B2CInteractiveBrowserCredentialOptions)?.BrowserCustomization;
    }



    /// <summary>
    /// Interactively authenticates a user via the default browser. The resulting <see cref="AuthenticationRecord"/> will automatically be used in subsequent calls to <see cref="GetToken"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <returns>The <see cref="AuthenticationRecord"/> of the authenticated account.</returns>
    public virtual AuthenticationRecord? Authenticate(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        return AuthenticateImplAsync(false, requestContext, cancellationToken).EnsureCompleted();
    }

    /// <summary>
    /// Interactively authenticates a user via the default browser.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <returns>The <see cref="AuthenticationRecord"/> of the authenticated account.</returns>
    public virtual async Task<AuthenticationRecord?> AuthenticateAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        return await AuthenticateImplAsync(true, requestContext, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Obtains an <see cref="AccessToken"/> token for a user account silently if the user has already authenticated, otherwise the
    /// default browser is launched to authenticate the user. Acquired tokens are cached by the credential instance. Token lifetime and
    /// refreshing is handled automatically. Where possible, reuse credential instances to optimize cache effectiveness.
    /// </summary>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls.</returns>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenImplAsync(false, requestContext, cancellationToken).EnsureCompleted();
    }

    /// <summary>
    /// Obtains an <see cref="AccessToken"/> token for a user account silently if the user has already authenticated, otherwise the
    /// default browser is launched to authenticate the user. Acquired tokens are cached by the credential instance. Token lifetime and
    /// refreshing is handled automatically. Where possible, reuse credential instances to optimize cache effectiveness.
    /// </summary>
    /// <param name="requestContext">The details of the authentication request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls.</returns>
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return await GetTokenImplAsync(true, requestContext, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AuthenticationRecord?> AuthenticateImplAsync(bool async, TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        await GetTokenViaBrowserLoginAsync(requestContext, async, cancellationToken).ConfigureAwait(false);
        return Record;

    }

    private async ValueTask<AccessToken> GetTokenImplAsync(bool async, TokenRequestContext requestContext, CancellationToken cancellationToken)
    {

        Exception? inner = null;

        if (Record is not null)
        {
            try
            {
                var result = await Client
                    .AcquireTokenSilentAsync(requestContext.Scopes, requestContext.Claims, Record, async, cancellationToken)
                    .ConfigureAwait(false);

                return new AccessToken(result.AccessToken, result.ExpiresOn);
            }
            catch (MsalUiRequiredException e)
            {
                inner = e;
            }
        }

        if (DisableAutomaticAuthentication)
        {
            throw new AuthenticationRequiredException(AuthenticationRequiredMessage, requestContext, inner);
        }

        return await GetTokenViaBrowserLoginAsync(requestContext, async, cancellationToken).ConfigureAwait(false);

    }

    private async Task<AccessToken> GetTokenViaBrowserLoginAsync(TokenRequestContext context, bool async, CancellationToken cancellationToken)
    {
        var prompt = LoginHint switch
        {
            null => Prompt.SelectAccount,
            _ => Prompt.NoPrompt
        };

        var result = await Client
            .AcquireTokenInteractiveAsync(context.Scopes, context.Claims, prompt, LoginHint, BrowserCustomization, async, cancellationToken)
            .ConfigureAwait(false);

        Record = new AuthenticationRecord(result, ClientId);
        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }
}