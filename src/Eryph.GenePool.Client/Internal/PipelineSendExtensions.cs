using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client.Internal;

internal static class PipelineSendExtensions
{
    public static async ValueTask<Response<TResponse>> SendRequestAsync<TResponse>(this HttpPipeline pipeline, HttpMessage message,
        CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        try
        {
            await pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return await message.DeserializeResponseAsync<TResponse>(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            message.Dispose();
        }
    }

    public static Response<TResponse> SendRequest<TResponse>(this HttpPipeline pipeline, HttpMessage message, CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        try
        {
            pipeline.Send(message, cancellationToken);
            return message.DeserializeResponse<TResponse>();
        }
        finally
        {
            message.Dispose();
        }
    }

}