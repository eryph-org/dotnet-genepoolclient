using Azure.Core;
using Eryph.GenePool.Client;
using Eryph.GenePool.Client.Credentials;

namespace Eryph.Packer
{
    internal static class AuthProvider
    {
        public static async Task<TokenCredential> GetInteractiveCredential(bool stagingAuthority)
        {
            var credentialOptions = new B2CInteractiveBrowserCredentialOptions
            {
                TokenCachePersistenceOptions = new TokenCachePersistenceOptions
                {
                    Name = stagingAuthority ? "packer-staging.cache" : "packer.cache" ,
                },
                ClientId = stagingAuthority ? "56136c3f-d46e-4644-a66c-b88304d09da8" : "0cadd98d-1e87-467b-a908-db1e340e9049",
                AuthorityUri = stagingAuthority ?
                    "https://dbosoftb2cstaging.b2clogin.com/tfp/dbosoftb2cstaging.onmicrosoft.com/B2C_1A_SIGNUP_SIGNIN/v2.0"
                    : "https://login.dbosoft.eu/tfp/a18f6025-dca7-463e-b38a-84cf9f2ca684/B2C_1A_SIGNUP_SIGNIN/v2.0"
            };

            var credential = new B2CInteractiveBrowserCredential(credentialOptions);
            var authCachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "eryph", "packer", stagingAuthority ? "auth-staging.cache": "auth.cache");

            var authCacheDir = Path.GetDirectoryName(authCachePath) ?? "";
            if (!Directory.Exists(authCacheDir))
                Directory.CreateDirectory(authCacheDir);

            AuthenticationRecord? record = null;
            if (File.Exists(authCachePath))
            {
                try
                {
                    await using var authRecordStream = new FileStream(authCachePath, FileMode.Open, FileAccess.Read);
                    record = await AuthenticationRecord.DeserializeAsync(authRecordStream);
                }
                catch
                {
                    // ignored
                }

            }

            if (record == null)
            {
                record = await credential.AuthenticateAsync(new TokenRequestContext());
                record?.SerializeAsync(File.OpenWrite(authCachePath));
            }

            credentialOptions.AuthenticationRecord = record;
            return new B2CInteractiveBrowserCredential(credentialOptions);

        }

        public static async Task<(ApiKeyCredential? ApiKey, TokenCredential? Token)> 
            GetCredential(string? apiKey, bool stagingAuthority)
        {
            if (apiKey == null)
                return (null, await GetInteractiveCredential(stagingAuthority));

            return (new ApiKeyCredential(apiKey), null);
        }
    }
}
