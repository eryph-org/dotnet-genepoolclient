using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using Azure;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Model.Requests.Organizations;

namespace Eryph.GenePool.Client.RestClients
{
    internal class OrganizationsRestClient
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
        public OrganizationsRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
        {
            ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _endpoint = endpoint;
            _version = version.ToString().ToLowerInvariant();
        }

        internal HttpMessage CreateRequest(OrganizationName organization, RequestMethod method)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = method;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            // ReSharper disable once StringLiteralTypo
            uri.AppendPath("/orgs/", false);
            uri.AppendPath(organization.Value, true);
            request.Uri = uri;
            request.Headers.Add("Accept", "application/json, text/json");
            return message;
        }

        /// <summary> Deletes a organization. </summary>
        /// <param name="organization"> The organization name</param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        public async Task<Response<NoResultResponse>> DeleteAsync(OrganizationName organization,
            RequestOptions options,
            CancellationToken cancellationToken = default)
        {
            if (organization == null)
                throw new ArgumentNullException(nameof(organization));

            
            return await _pipeline.SendRequestAsync<NoResultResponse>(
                CreateRequest(organization, RequestMethod.Delete), options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary> Deletes a organization. </summary>
        /// <param name="organization"> The organization name</param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        public Response<NoResultResponse> Delete(OrganizationName organization,
            RequestOptions options,
            CancellationToken cancellationToken = default)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            return _pipeline.SendRequest<NoResultResponse>(CreateRequest(organization, RequestMethod.Delete),
                options,
                cancellationToken);
        }

        /// <summary> Get a organization. </summary>
        /// <param name="organization"> The organization name</param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        public async Task<Response<SingleResultResponse<OrganizationResponse>>> GetAsync(
            OrganizationName organization, RequestOptions options,
            CancellationToken cancellationToken = default)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            return await _pipeline.SendRequestAsync<SingleResultResponse<OrganizationResponse>>(
                CreateRequest(organization, RequestMethod.Get), options, cancellationToken).ConfigureAwait(false);

        }

        /// <summary> Get a organization. </summary>
        /// <param name="organization"> The organization name</param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        public Response<SingleResultResponse<OrganizationResponse>> Get(
            OrganizationName organization, RequestOptions options,
            CancellationToken cancellationToken = default)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            return _pipeline.SendRequest<SingleResultResponse<OrganizationResponse>>(
                CreateRequest(organization, RequestMethod.Get), options, cancellationToken);

        }


        // currently not supported, will be added later

        ///// <summary> Updates a organization. </summary>
        ///// <param name="organization"> The organization to update. </param>
        ///// <param name="body"> The UpdateOrganizationBody to use. </param>
        ///// <param name="cancellationToken"> The cancellation token to use. </param>
        ///// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        //public async Task<SingleResultResponse<OrganizationRefResponse>> UpdateAsync(OrganizationName organization,
        //    UpdateOrganizationBody? body = null, CancellationToken cancellationToken = default)
        //{
        //    if (organization == null)
        //    {
        //        throw new ArgumentNullException(nameof(organization));
        //    }

        //    using var message = CreateRequest(organization, RequestMethod.Patch);
        //    if (body != null)
        //    {
        //        message.Request.Headers.Add("Content-Type", "application/json");
        //        var content = new Utf8JsonRequestContent();
        //        content.JsonWriter.WriteObjectValue(body);
        //        message.Request.Content = content;
        //    }

        //    return await _pipeline.SendRequestAsync<SingleResultResponse<OrganizationRefResponse>>(message, cancellationToken).ConfigureAwait(false);
        //}


        ///// <summary> Updates a organization. </summary>
        ///// <param name="organization"> The organization name</param>
        ///// <param name="body"> The UpdateOrganizationBody to use. </param>
        ///// <param name="cancellationToken"> The cancellation token to use. </param>
        ///// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
        //public SingleResultResponse<OrganizationRefResponse> Update(OrganizationName organization, UpdateOrganizationBody? body = null, CancellationToken cancellationToken = default)
        //{
        //    if (organization == null)
        //    {
        //        throw new ArgumentNullException(nameof(organization));
        //    }

        //    using var message = CreateRequest(organization, RequestMethod.Patch);
        //    if (body != null)
        //    {
        //        message.Request.Headers.Add("Content-Type", "application/json");
        //        var content = new Utf8JsonRequestContent();
        //        content.JsonWriter.WriteObjectValue(body);
        //        message.Request.Content = content;
        //    }

        //    return _pipeline.SendRequest<SingleResultResponse<OrganizationRefResponse>>(message, cancellationToken);

        //}

        internal HttpMessage CreateNewOrgRequest(CreateOrganizationBody? body)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Post;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            // ReSharper disable once StringLiteralTypo
            uri.AppendPath("/orgs", false);
            request.Uri = uri;
            request.Headers.Add("Accept", "application/json, text/json");
            if (body != null)
            {
                request.Headers.Add("Content-Type", "application/json");
                var content = new Utf8JsonRequestContent();
                content.JsonWriter.WriteObjectValue(body);
                request.Content = content;
            }
            return message;
        }

        /// <summary> Creates a organization. </summary>
        /// <param name="body"> The CreateOrganizationBody to use. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public async Task<Response<SingleResultResponse<OrganizationRefResponse>>> CreateAsync(
            CreateOrganizationBody body, RequestOptions options
            , CancellationToken cancellationToken = default)
        {

            using var message = CreateNewOrgRequest(body);
            return await _pipeline.SendRequestAsync<SingleResultResponse<OrganizationRefResponse>>(message,
                options, cancellationToken).ConfigureAwait(false);
        }


        /// <summary> Creates a organization. </summary>
        /// <param name="body"> The CreateOrganizationBody to use. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public Response<SingleResultResponse<OrganizationRefResponse>> Create(CreateOrganizationBody body, RequestOptions options, CancellationToken cancellationToken = default)
        {

            using var message = CreateNewOrgRequest(body);
            return _pipeline.SendRequest<SingleResultResponse<OrganizationRefResponse>>(message,
                options, cancellationToken);

        }


    }
}
