using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Credentials;

namespace Eryph.GenePool.Client;

internal sealed class ApiKeyPipelinePolicy : HttpPipelineSynchronousPolicy
{

    /// <summary>
    /// Shared key credentials used to sign requests
    /// </summary>
    private readonly ApiKeyCredential _credentials;

    /// <summary>
    /// Create a new SharedKeyPipelinePolicy
    /// </summary>
    /// <param name="credentials">SharedKeyCredentials to authenticate requests.</param>
    public ApiKeyPipelinePolicy(ApiKeyCredential credentials)
        => _credentials = credentials;

    /// <summary>
    /// Sign the request using the shared key credentials.
    /// </summary>
    /// <param name="message">The message with the request to sign.</param>
    public override void OnSendingRequest(HttpMessage message)
    {
        base.OnSendingRequest(message);
        message.Request.Headers.SetValue("X-Api-Key", _credentials.ApiKey);
    }

}