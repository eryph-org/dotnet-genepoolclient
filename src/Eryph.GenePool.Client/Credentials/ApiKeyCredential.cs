namespace Eryph.GenePool.Client.Credentials;

public class ApiKeyCredential(string apiKey)
{
    public string ApiKey { get; } = apiKey;
}