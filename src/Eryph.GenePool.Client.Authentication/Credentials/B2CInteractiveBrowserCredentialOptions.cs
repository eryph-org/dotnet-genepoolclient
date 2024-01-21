using System;
using System.Threading;
using Azure.Core;

namespace Eryph.GenePool.Client.Credentials;

/// <summary>
/// Options to configure the <see cref="B2CInteractiveBrowserCredential"/>.
/// </summary>
public class B2CInteractiveBrowserCredentialOptions : TokenCredentialOptions
{
    /// <summary>
    /// Prevents the <see cref="B2CInteractiveBrowserCredential"/> from automatically prompting the user. If automatic authentication is disabled a AuthenticationRequiredException will be thrown from <see cref="B2CInteractiveBrowserCredential.GetToken"/> and <see cref="B2CInteractiveBrowserCredential.GetTokenAsync"/> in the case that
    /// user interaction is necessary. The application is responsible for handling this exception, and calling <see cref="B2CInteractiveBrowserCredential.Authenticate(TokenRequestContext,CancellationToken)"/> or <see cref="B2CInteractiveBrowserCredential.AuthenticateAsync(TokenRequestContext,CancellationToken)"/> to authenticate the user interactively.
    /// </summary>
    public bool DisableAutomaticAuthentication { get; set; }


    /// <summary>
    /// The client ID of the application used to authenticate the user. If not specified the user will be authenticated with an Azure development application.
    /// </summary>
    public string? ClientId { get; set; }


    /// <summary>
    /// Uri where the STS will call back the application with the security token. This parameter is not required if the caller is not using a custom <see cref="ClientId"/>. In
    /// the case that the caller is using their own <see cref="ClientId"/> the value must match the redirect url specified when creating the application registration.
    /// </summary>
    public Uri? RedirectUri { get; set; }

    /// <summary>
    /// The <see cref="AuthenticationRecord"/> captured from a previous authentication.
    /// </summary>
    public AuthenticationRecord? AuthenticationRecord { get; set; }

    /// <summary>
    /// Avoids the account prompt and pre-populates the username of the account to login.
    /// </summary>
    public string? LoginHint { get; set; }


    /// <summary>
    /// The options for customizing the browser for interactive authentication.
    /// </summary>
    public BrowserCustomizationOptions? BrowserCustomization { get; set; }
}