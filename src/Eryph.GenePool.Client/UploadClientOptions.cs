using Azure.Core;
using Azure.Core.Pipeline;

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