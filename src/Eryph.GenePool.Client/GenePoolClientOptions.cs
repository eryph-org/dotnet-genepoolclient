using System;
using System.Linq;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Credentials;

namespace Eryph.GenePool.Client;


public class UploadClientOptions : ClientOptions
{
    public HttpPipeline Build()
    {
        var pipelineOptions = new HttpPipelineOptions(this)
        {
            // PerCallPolicies = { new  },
            //// needed *after* core applies the user agent; can't have that without going per-retry
            //PerRetryPolicies = { StorageTelemetryPolicy.Shared },
            //ResponseClassifier = classifier,
            //RequestFailedDetailsParser = new StorageRequestFailedDetailsParser()
        };

        return HttpPipelineBuilder.Build(pipelineOptions);
    }

    /// <summary>
    /// Configures how many threads should be used to upload a single gene.
    /// Default is 2.
    /// </summary>
    public int MaxParallelThreads { get; set; } = 2;
}

/// <summary>
/// Provides the client configuration options for connecting to Azure Blob
/// Storage.
/// </summary>
public class GenePoolClientOptions : ClientOptions
{
    /// <summary>
    /// The Latest service version supported by this client library.
    /// </summary>
    internal const ServiceVersion LatestVersion = ServiceVersion.V1;
    internal const string DefaultEndpoint = "https://genepool-api.eryph.io";

    public enum ServiceVersion
    {
        V1 = 1
    }

    public ServiceVersion Version { get; }
    public Uri Endpoint { get; }

    public string[] Scopes { get; set; } = new[]
    {
        ScopeNames.OrgReadWrite, ScopeNames.GenesetReadWrite
    };

    public GenePoolClientOptions(ServiceVersion version = LatestVersion, string endpoint = DefaultEndpoint)
    {
        Version = version;
        Endpoint = new Uri(endpoint);
        AddHeadersAndQueryParameters();
    }

    /// <summary>
    /// Add headers and query parameters in <see cref="DiagnosticsOptions.LoggedHeaderNames"/> and <see cref="DiagnosticsOptions.LoggedQueryParameters"/>
    /// </summary>
    private void AddHeadersAndQueryParameters()
    {
        Diagnostics.LoggedHeaderNames.Add("Content-Disposition");
        Diagnostics.LoggedHeaderNames.Add("Content-Encoding");
        Diagnostics.LoggedHeaderNames.Add("Content-Language");
        Diagnostics.LoggedHeaderNames.Add("Content-MD5");
        Diagnostics.LoggedHeaderNames.Add("Content-Range");

        Diagnostics.LoggedQueryParameters.Add("org");
        Diagnostics.LoggedQueryParameters.Add("genesets");
        Diagnostics.LoggedQueryParameters.Add("gene");
        Diagnostics.LoggedQueryParameters.Add("tag");
    }

    public static HttpPipelinePolicy? GetAuthenticationPolicy(object? credentials, string scope)
    {
        // Use the credentials to decide on the authentication policy
        return credentials switch
        {
            null => // Anonymous authentication
                null,
            ApiKeyCredential sharedKey => sharedKey.AsPolicy(),
            TokenCredential token => token.AsPolicy(scope),
            _ => null
        };
    }

    public HttpPipeline Build(HttpPipelinePolicy? authentication = null)
    {
        //StorageResponseClassifier classifier = new();
        var pipelineOptions = new HttpPipelineOptions(this)
        {
            //PerCallPolicies = { StorageServerTimeoutPolicy.Shared },
            //// needed *after* core applies the user agent; can't have that without going per-retry
            //PerRetryPolicies = { StorageTelemetryPolicy.Shared },
            //ResponseClassifier = classifier,
            //RequestFailedDetailsParser = new StorageRequestFailedDetailsParser()
        };

        if(authentication != null)
            pipelineOptions.PerRetryPolicies.Add(authentication); // authentication needs to be the last of the perRetry client policies passed in to Build

        return HttpPipelineBuilder.Build(pipelineOptions);
    }

    public HttpPipeline Build(TokenCredential? tokenCredential, ApiKeyCredential? apiKeyCredential) =>
        Build(GetAuthenticationPolicy(tokenCredential == null ? apiKeyCredential: tokenCredential, BuildScopes()));



    private string BuildScopes()
    {
        return string.Join(" ", Scopes.Select(scopeName => $"https://dbosoftb2cstaging.onmicrosoft.com/bf5f63a8-7bd1-4c01-aadb-08dee019cec1/{scopeName}"));
    }
}