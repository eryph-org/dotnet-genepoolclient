using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Credentials;

namespace Eryph.GenePool.Client;

public static class CredentialPolicyExtensions
{

    public static HttpPipelinePolicy AsPolicy(this ApiKeyCredential credential) =>
        new ApiKeyPipelinePolicy(credential);

    /// <summary>
    /// Get an authentication policy to sign Storage requests.
    /// </summary>
    /// <param name="credential">Credential to use.</param>
    /// <param name="scope">Scope to use.</param>
    /// <returns>An authentication policy.</returns>
    public static HttpPipelinePolicy AsPolicy(this TokenCredential credential, string scope) =>
        new BearerTokenAuthenticationPolicy(
            credential,
            scope);

}