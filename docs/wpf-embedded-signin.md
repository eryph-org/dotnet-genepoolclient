# Embedded WebView2 sign-in for Windows desktop apps

By default `B2CInteractiveBrowserCredential` opens the system's default browser to
sign the user in. Windows desktop applications (WPF / WinForms) often prefer an
**embedded** sign-in dialog that appears as a child window of the application
instead.

MSAL implements the embedded dialog with WebView2, which lives in the separate
[`Microsoft.Identity.Client.Desktop`](https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop)
package. This client library targets `netstandard2.1` and therefore does **not**
reference that package itself. Instead it exposes the underlying MSAL
`PublicClientApplicationBuilder` so that a consuming desktop application can add
the package and enable the feature.

## 1. Add the package to your desktop app

```
dotnet add package Microsoft.Identity.Client.Desktop
```

The consuming project must target a Windows desktop TFM (for example
`net8.0-windows`) and have `<UseWPF>true</UseWPF>` (or `<UseWindowsForms>true</UseWindowsForms>`).

## 2. Configure the credential

Three pieces are required to show the embedded dialog modally over your window:

- `ConfigurePublicClientApplication` &ndash; call `WithWindowsEmbeddedBrowserSupport()`
  on the exposed builder (this method comes from the `Desktop` package).
- `BrowserCustomization.UseEmbeddedWebView = true` &ndash; select the embedded view
  instead of the system browser.
- `BrowserCustomization.ParentActivityOrWindow` &ndash; provide the window handle so
  the dialog is owned by your application window.

```csharp
using System.Windows.Interop;
using Eryph.GenePool.Client;
using Eryph.GenePool.Client.Credentials;
using Microsoft.Identity.Client;

// window is the WPF Window that should own the sign-in dialog
var windowHandle = new WindowInteropHelper(window).EnsureHandle();

var options = new B2CInteractiveBrowserCredentialOptions
{
    ClientId = "<your-client-id>",
    AuthorityUri = "<your-b2c-authority-uri>",

    // Enable the embedded WebView2 dialog from the Desktop package.
    ConfigurePublicClientApplication = builder =>
        builder.WithWindowsEmbeddedBrowserSupport(),

    BrowserCustomization = new BrowserCustomizationOptions
    {
        UseEmbeddedWebView = true,
        ParentActivityOrWindow = () => windowHandle,
    },
};

var credential = new B2CInteractiveBrowserCredential(options);
```

The credential can then be passed to `GenePoolClient` as usual. On non-Windows
platforms simply omit `ConfigurePublicClientApplication` and leave
`UseEmbeddedWebView` unset to keep using the system browser.
