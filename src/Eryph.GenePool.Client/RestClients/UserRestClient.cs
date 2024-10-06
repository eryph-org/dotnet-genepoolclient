using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using Azure;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model.Responses;
using Eryph.GenePool.Client.Requests;

namespace Eryph.GenePool.Client.RestClients
{
    internal class UserRestClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly Uri _endpoint;
        private readonly string _version;

        /// <summary> The ClientDiagnostics is used to provide tracing support for the client library. </summary>
        internal ClientDiagnostics ClientDiagnostics { get; }

        /// <summary> Initializes a new instance of VirtualDisksRestClient. </summary>
        /// <param name="clientDiagnostics"> The handler for diagnostic messaging in the client. </param>
        /// <param name="pipeline"> The HTTP pipeline for sending and receiving REST requests and responses. </param>
        /// <param name="endpoint"> server parameter. </param>
        /// <param name="version">The api version</param>
        /// <exception cref="ArgumentNullException"> <paramref name="clientDiagnostics"/> or <paramref name="pipeline"/> is null. </exception>
        public UserRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
        {
            ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _endpoint = endpoint;
            _version = version.ToString().ToLowerInvariant();
        }

        internal HttpMessage CreateRequest(GetUserRequestOptions options)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            // ReSharper disable once StringLiteralTypo
            uri.AppendPath("/user", false);
            uri.AppendPath("/me", false);
            uri.AddExpandOptions(options.Expand);

            request.Uri = uri;
            request.Headers.Add("Accept", "application/json, text/json");
            return message;
        }


        /// <summary> Get information for current user. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <param name="options">Request options</param>
        public async Task<Response<SingleResultResponse<GetMeResponse>>> GetAsync(
            GetUserRequestOptions options,
            CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<SingleResultResponse<GetMeResponse>>(CreateRequest(options)
                ,options, cancellationToken).ConfigureAwait(false);

        }

        /// <summary> Get information for current user. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <param name="options">Request options</param>
        public Response<SingleResultResponse<GetMeResponse>> Get(
            GetUserRequestOptions options, 
            CancellationToken cancellationToken = default)
        {
            return _pipeline.SendRequest<SingleResultResponse<GetMeResponse>>(
                CreateRequest(options), options, cancellationToken);

        }



    }
}
