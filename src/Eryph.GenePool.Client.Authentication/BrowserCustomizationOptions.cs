using System;
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

    /// <summary>
    /// A callback which returns the handle of the window that should own the interactive
    /// authentication dialog. This is required when using the embedded WebView2 dialog on a
    /// Windows desktop (WPF/WinForms) application so that the sign-in window is displayed
    /// modally over the application. For WPF the handle can be obtained via
    /// <c>new WindowInteropHelper(window).EnsureHandle()</c>, which also creates the handle if
    /// the window has not been shown yet (unlike <c>Handle</c>, which returns
    /// <c>IntPtr.Zero</c> in that case).
    /// </summary>
    public Func<IntPtr>? ParentActivityOrWindow { get; set; }

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