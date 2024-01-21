using Azure.Core;
using Eryph.GenePool.Client;
using Eryph.GenePool.Client.Credentials;

namespace Eryph.Packer
{
    internal static class AuthProvider
    {
        public static async Task<TokenCredential> GetInteractiveCredential()
        {
            var credentialOptions = new B2CInteractiveBrowserCredentialOptions
            {
                TokenCachePersistenceOptions = new TokenCachePersistenceOptions
                {
                    Name = "eryph.cache",
                },
                ClientId = "56136c3f-d46e-4644-a66c-b88304d09da8",
                AuthorityUri =
                    "https://dbosoftb2cstaging.b2clogin.com/tfp/dbosoftb2cstaging.onmicrosoft.com/B2C_1A_SIGNUP_SIGNIN/v2.0"
            };

            var credential = new B2CInteractiveBrowserCredential(credentialOptions);
            var authCachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "eryph", "packer", "auth.cache");

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
            GetCredential(string? apiKey)
        {
            if (apiKey == null)
                return (null, await GetInteractiveCredential());

            return (new ApiKeyCredential(apiKey), null);
        }
    }
}
