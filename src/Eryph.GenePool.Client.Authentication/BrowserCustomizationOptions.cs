using JetBrains.Annotations;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

/// <summary>
/// Options to customize browser view.
/// </summary>
[PublicAPI]
public class BrowserCustomizationOptions
{
    /// <summary>
    /// Specifies if the public client application should use an embedded web browser
    /// or the system default browser
    /// </summary>
    public bool? UseEmbeddedWebView { get; set; }

    internal SystemWebViewOptions? SystemBrowserOptions;

    private SystemWebViewOptions SystemWebViewOptions
    {
        get
        {
            SystemBrowserOptions ??= new SystemWebViewOptions();
            return SystemBrowserOptions;
        }
    }

    /// <summary>
    /// Property to set HtmlMessageSuccess of SystemWebViewOptions from MSAL,
    /// which the browser will show to the user when the user finishes authenticating successfully.
    /// </summary>
    public string SuccessMessage
    {
        get => SystemWebViewOptions.HtmlMessageSuccess;

        set => SystemWebViewOptions.HtmlMessageSuccess = value;
    }

    /// <summary>
    /// Property to set HtmlMessageError of SystemWebViewOptions from MSAL,
    /// which the browser will show to the user when the user finishes authenticating, but an error occurred.
    /// You can use a string format e.g. "An error has occurred: {0} details: {1}".
    /// </summary>
    public string ErrorMessage
    {
        get => SystemWebViewOptions.HtmlMessageError;

        set => SystemWebViewOptions.HtmlMessageError = value;
    }
}