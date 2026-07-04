using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

internal class MsalPublicClient(
    string authorityUri,
    string? clientId,
    string redirectUrl,
    TokenCredentialOptions? options,
    Action<PublicClientApplicationBuilder>? configureBuilder = null)
    : MsalClientBase<IPublicClientApplication>(authorityUri, clientId, options?.TokenCachePersistenceOptions)
{
    internal string RedirectUrl { get; } = redirectUrl;

    private readonly Action<PublicClientApplicationBuilder>? _configureBuilder = configureBuilder;

    protected override ValueTask<IPublicClientApplication> CreateClientAsync(bool async, CancellationToken cancellationToken)
    {
        return CreateClientCoreAsync(async, cancellationToken);
    }

    protected virtual ValueTask<IPublicClientApplication> CreateClientCoreAsync(bool async, CancellationToken cancellationToken)
    {
        var pubAppBuilder = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithB2CAuthority(AuthorityUri);


        if (!string.IsNullOrEmpty(RedirectUrl))
        {
            pubAppBuilder = pubAppBuilder.WithRedirectUri(RedirectUrl);
        }

        // Allow the consuming application to apply additional configuration to the builder,
        // e.g. to enable the embedded WebView2 sign-in dialog via the
        // Microsoft.Identity.Client.Desktop package.
        _configureBuilder?.Invoke(pubAppBuilder);

        return new ValueTask<IPublicClientApplication>(pubAppBuilder.Build());
    }

    public async ValueTask<AuthenticationResult> AcquireTokenSilentAsync(string[] scopes, string? claims, AuthenticationRecord record, bool async, CancellationToken cancellationToken)
    {
        var result = await AcquireTokenSilentCoreAsync(scopes, claims, record, async, cancellationToken).ConfigureAwait(false);
        return result;
    }

    protected virtual async ValueTask<AuthenticationResult> AcquireTokenSilentCoreAsync(string[] scopes, string? claims, AuthenticationRecord record, bool async, CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(async, cancellationToken).ConfigureAwait(false);

        // if the user specified a TenantId when they created the client we want to authenticate to that tenant.
        // otherwise we should authenticate with the tenant specified by the authentication record since that's the tenant the
        // user authenticated to originally.
        return await client.AcquireTokenSilent(scopes, (AuthenticationAccount)record)
            .WithClaims(claims ?? "")
            .ExecuteAsync(async, cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes, string? claims, Prompt prompt, string? loginHint, BrowserCustomizationOptions? browserOptions, bool async, CancellationToken cancellationToken)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {

            return Task.Run(async () =>
            {
                var result = await AcquireTokenInteractiveCoreAsync(scopes, claims, prompt, loginHint, browserOptions, true, cancellationToken).ConfigureAwait(false);
                return result;
            }, cancellationToken).GetAwaiter().GetResult();
        }

        var result = await AcquireTokenInteractiveCoreAsync(scopes, claims, prompt, loginHint, browserOptions, async, cancellationToken).ConfigureAwait(false);
        return result;
    }

    protected virtual async ValueTask<AuthenticationResult> AcquireTokenInteractiveCoreAsync(string[] scopes, string? claims, Prompt prompt, string? loginHint, BrowserCustomizationOptions? browserOptions, bool async, CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(async, cancellationToken).ConfigureAwait(false);

        var builder = client.AcquireTokenInteractive(scopes)
            .WithPrompt(prompt)
            .WithClaims(claims ?? "");
        if (loginHint != null)
        {
            builder.WithLoginHint(loginHint);
        }

        // ReSharper disable once InvertIf
        if (browserOptions != null)
        {
            if (browserOptions.UseEmbeddedWebView.HasValue)
            {
                builder.WithUseEmbeddedWebView(browserOptions.UseEmbeddedWebView.Value);
            }
            if (browserOptions.SystemBrowserOptions != null)
            {
                builder.WithSystemWebViewOptions(browserOptions.SystemBrowserOptions);
            }
            if (browserOptions.ParentActivityOrWindow != null)
            {
                // Resolve the handle and pass the IntPtr value: MSAL only sets the owner window for
                // an IntPtr, not for a Func<IntPtr>. The callback may run on a non-UI thread here
                // (AcquireTokenInteractiveAsync offloads to Task.Run when called from an STA thread),
                // so a handle resolved with UI-thread affinity can throw. The parent window is
                // optional, so treat a failure or a zero handle as "no parent" and let MSAL show an
                // unowned dialog rather than failing the sign-in.
                var parentWindow = IntPtr.Zero;
                try
                {
                    parentWindow = browserOptions.ParentActivityOrWindow();
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // Ignore: fall back to an unowned dialog.
                }

                if (parentWindow != IntPtr.Zero)
                {
                    builder.WithParentActivityOrWindow(parentWindow);
                }
            }
        }
        return await builder
            .ExecuteAsync(async, cancellationToken)
            .ConfigureAwait(false);
    }

}