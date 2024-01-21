using System;
using System.Collections.Generic;
using System.IO;

namespace Eryph.GenePool.Client;

internal class Constants
{
 
    public const string DefaultRedirectUrl = "http://localhost";

    public static readonly string DefaultMsalTokenCacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "Eryph",
        ".tokens");

    public const string DefaultMsalTokenCacheKeychainService = "Microsoft.Developer.IdentityService";

    public const string DefaultMsalTokenCacheKeyringSchema = "msal.cache";

    public const string DefaultMsalTokenCacheKeyringCollection = "default";

    public static readonly KeyValuePair<string, string> DefaultMsalTokenCacheKeyringAttribute1 = 
        new("MsalClientID", "Microsoft.Developer.IdentityService");

    public static readonly KeyValuePair<string, string> DefaultMsalTokenCacheKeyringAttribute2 = 
        new("Microsoft.Developer.IdentityService", "1.0.0.0");

    public const string DefaultEryphTokenCacheName = "eryph_clients.cache";

}