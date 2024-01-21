using System.Diagnostics;
using Azure.Core;
using Eryph.GenePool.Client;
using Eryph.GenePool.Client.Credentials;

namespace ClientTestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var credentialOptions = new B2CInteractiveBrowserCredentialOptions
            {
                TokenCachePersistenceOptions = new TokenCachePersistenceOptions
                {
                    Name = "some.cache",
                },
                ClientId = "56136c3f-d46e-4644-a66c-b88304d09da8",
                AuthorityUri =
                    "https://dbosoftb2cstaging.b2clogin.com/tfp/dbosoftb2cstaging.onmicrosoft.com/B2C_1A_SIGNUP_SIGNIN/v2.0"
            };
            var credential = new B2CInteractiveBrowserCredential(credentialOptions);

            AuthenticationRecord? record = null;
            if (File.Exists("record"))
            {
                await using var authRecordStream = new FileStream("record", FileMode.Open, FileAccess.Read);
                record = await AuthenticationRecord.DeserializeAsync(authRecordStream);
            }

            if (record == null)
            {
                record = await credential.AuthenticateAsync(new TokenRequestContext());
                record?.SerializeAsync(File.OpenWrite("record"));

            }

            credentialOptions.AuthenticationRecord = record;
            credential = new B2CInteractiveBrowserCredential(credentialOptions);

            var genePoolClient = new GenePoolClient(
                //new Uri("http://localhost:7072/api/"),
                new Uri("https://eryphgenepoolapistaging.azurewebsites.net/api/"),
                //
                //new ApiKeyCredential("ARFtuoXppGWESoM4qnTjQJ8LdTZAf/XjBUS5BFEOv5O6TyM/iwM6/YVXCCEYdxUdlNxy"));
                credential);
            var orgClient = genePoolClient.GetOrganizationsClient("dbosoft");

            Console.WriteLine(await orgClient.GetAsync());

            var sw = Stopwatch.StartNew();
            //var response = await genePoolClient.CreateGeneFromPathAsync("dbosoft/ubuntu-jammy/20231024",
            //    "D:\\Source\\Repos\\eryph\\build-images\\genes\\dbosoft\\winsrv2019-standard\\20231207\\d5693cbdc2756d6abb28709273f4db57ee77709b7d86901c9dd2136877aefc3f"
            //);


            var genesetClient = genePoolClient.GetGenesetTagClient("dbosoft/ubuntu-jammy/20231024");
            var downloadResponse = await genesetClient.GetForDownloadAsync();
            Console.WriteLine(downloadResponse);

            Console.WriteLine(sw.Elapsed);
        }
    }
}
