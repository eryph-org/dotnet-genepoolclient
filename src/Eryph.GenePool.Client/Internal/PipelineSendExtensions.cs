using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client.Internal;

internal static class PipelineSendExtensions
{
    public static async ValueTask<TResponse> SendRequestAsync<TResponse>(this HttpPipeline pipeline, HttpMessage message,
        CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        await pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
        return await message.DeserializeResponseAsync<TResponse>(cancellationToken).ConfigureAwait(false);
    
    }

    public static TResponse SendRequest<TResponse>(this HttpPipeline pipeline, HttpMessage message, CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        pipeline.Send(message, cancellationToken);
        return message.DeserializeResponse<TResponse>();
    }

}