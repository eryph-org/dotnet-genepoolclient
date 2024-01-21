namespace Eryph.GenePool.Client.Credentials;

public class ApiKeyCredential
{
    public string ApiKey { get; }


    public ApiKeyCredential(string apiKey)
    {
        ApiKey = apiKey;
    }

}