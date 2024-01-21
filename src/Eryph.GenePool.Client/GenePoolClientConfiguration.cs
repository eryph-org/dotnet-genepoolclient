using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Credentials;
using Eryph.GenePool.Client.Internal;

namespace Eryph.GenePool.Client;

/// <summary>
/// Provides the configurations to connecting to the Blob Service and to create the Blob Clients
/// </summary>
internal class GenePoolClientConfiguration
{

    public GenePoolClientOptions.ServiceVersion Version { get; internal set; }

    public HttpPipeline Pipeline { get; private set; }
    public HttpPipeline UploadPipeline { get; private set; }
    public UploadClientOptions UploadOptions { get; private set; }


    public ApiKeyCredential? ApiKeyCredential { get; private set; }

    public TokenCredential? TokenCredential { get; private set; }

    public ClientDiagnostics ClientDiagnostics { get; private set; }


    /// <summary>
    /// Create a <see cref="GenePoolClientConfiguration"/> with token authentication.
    /// </summary>

    public GenePoolClientConfiguration(
        HttpPipeline pipeline,
        HttpPipeline uploadPipeline,
        UploadClientOptions uploadOptions,
        ApiKeyCredential? apiKeyCredential,
        TokenCredential? tokenCredential,
        ClientDiagnostics clientDiagnostics,
        GenePoolClientOptions.ServiceVersion version)
    {
        Version = version;
        Pipeline = pipeline;
        UploadPipeline = uploadPipeline;
        UploadOptions = uploadOptions;
        ApiKeyCredential = apiKeyCredential;
        TokenCredential = tokenCredential;
        ClientDiagnostics = clientDiagnostics;
    }

}