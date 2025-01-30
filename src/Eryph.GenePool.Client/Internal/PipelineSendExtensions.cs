using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.Responses;

namespace Eryph.GenePool.Client.Internal;

internal static class PipelineSendExtensions
{
    public static async ValueTask<Response<TResponse>> SendRequestAsync<TResponse>(
        this HttpPipeline pipeline, 
        HttpMessage message,
        RequestOptions requestOptions,
        CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        try
        {
            await pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return await message.DeserializeResponseAsync<TResponse>(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (
            ex is RequestFailedException or IOException
                or AggregateException { InnerException: RequestFailedException or IOException })
        {
            throw CreateException(ex);
        }
        finally
        {
            if (message.HasResponse) 
                requestOptions.OnResponse?.Invoke(message.Response);

            message.Dispose();
        }
    }

    public static Response<TResponse> SendRequest<TResponse>(
        this HttpPipeline pipeline, 
        HttpMessage message,
        RequestOptions requestOptions,
        CancellationToken cancellationToken)
        where TResponse : ResponseBase
    {
        try
        {
            pipeline.Send(message, cancellationToken);
            return message.DeserializeResponse<TResponse>();
        }
        catch (Exception ex) when (
            ex is RequestFailedException or IOException
                or AggregateException { InnerException: RequestFailedException or IOException })
        {
            throw CreateException(ex);
        }
        finally
        {
            if (message.HasResponse)
                requestOptions.OnResponse?.Invoke(message.Response);

            message.Dispose();
        }
    }

    private static GenepoolClientException CreateException(Exception exception)
    {
        var message = exception switch
        {
            // When a retry policy is used, the exceptions of all attempts are wrapped in
            // an AggregateException.
            AggregateException ae => string.IsNullOrWhiteSpace(ae.InnerException?.Message)
                ? ae.Message
                : ae.InnerException.Message,
            _ => exception.Message,
        };
        
        return new GenepoolClientException(
            $"The request failed without a proper response: {message}",
            exception);
    }
}
