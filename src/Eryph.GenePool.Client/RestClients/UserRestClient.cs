using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using Azure;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model.Responses;

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

        internal HttpMessage CreateRequest(bool expandIdentityOrgs, bool expandGenepoolOrgs)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            // ReSharper disable once StringLiteralTypo
            uri.AppendPath("/me", false);

            if (expandGenepoolOrgs || expandIdentityOrgs)
            {
                var expand = new System.Text.StringBuilder();
                if (expandGenepoolOrgs)
                {
                    expand.Append("genepool_orgs");
                }

                if (expandIdentityOrgs)
                {
                    if(expand.Length>0)
                        expand.Append(",");
                    expand.Append("identity_orgs");
                }

                uri.AppendQuery("expand", expand.ToString());
            }

            request.Uri = uri;
            request.Headers.Add("Accept", "application/json, text/json");
            return message;
        }


        /// <summary> Get information for current user. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <param name="expandGenepoolOrgs">Gets the genepool organization names that the user is a member of.</param>
        /// <param name="expandIdentityOrgs">Gets details for identity organizations that the user is a member of.</param>
        public async Task<Response<SingleResultResponse<GetMeResponse>>> GetAsync(
            bool expandGenepoolOrgs = false, bool expandIdentityOrgs = false,
            CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<SingleResultResponse<GetMeResponse>>(CreateRequest(
                expandIdentityOrgs, expandGenepoolOrgs), cancellationToken).ConfigureAwait(false);

        }

        /// <summary> Get information for current user. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <param name="expandGenepoolOrgs">Gets the genepool organization names that the user is a member of.</param>
        /// <param name="expandIdentityOrgs">Gets details for identity organizations that the user is a member of.</param>
        public Response<SingleResultResponse<GetMeResponse>> Get(
            bool expandGenepoolOrgs = false, bool expandIdentityOrgs = false, 
            CancellationToken cancellationToken = default)
        {
            return _pipeline.SendRequest<SingleResultResponse<GetMeResponse>>(
                CreateRequest(expandGenepoolOrgs, expandIdentityOrgs), cancellationToken);

        }



    }
}
