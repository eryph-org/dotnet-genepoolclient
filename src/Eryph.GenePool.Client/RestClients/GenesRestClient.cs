using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.RestClients
{
    internal class GenesRestClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly Uri _endpoint;
        private readonly string _version;

        /// <summary> The ClientDiagnostics is used to provide tracing support for the client library. </summary>
        internal ClientDiagnostics ClientDiagnostics { get; }

        /// <summary> Initializes a new instance of GenesRestClient. </summary>
        /// <param name="clientDiagnostics"> The handler for diagnostic messaging in the client. </param>
        /// <param name="pipeline"> The HTTP pipeline for sending and receiving REST requests and responses. </param>
        /// <param name="endpoint"> server parameter. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="clientDiagnostics"/> or <paramref name="pipeline"/> is null. </exception>
        public GenesRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
        {
            ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _endpoint = endpoint;
            _version = version.ToString().ToLowerInvariant();
        }

        internal HttpMessage CreateRequest(GeneSetIdentifier identifier, Gene gene, RequestMethod method)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = method;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            uri.AppendPath("/genes/", false);
            uri.AppendPath(identifier.Value, false);
            uri.AppendPath("/", false);
            uri.AppendPath(gene.Value, true);
            request.Uri = uri;
            request.Headers.Add("Accept", "application/json, text/json");
            return message;
        }

        public async Task<NoResultResponse> DeleteAsync(GeneSetIdentifier geneset, Gene gene, CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(geneset, gene, RequestMethod.Delete), cancellationToken).ConfigureAwait(false);
        }

        public NoResultResponse Delete(GeneSetIdentifier geneset, Gene gene, CancellationToken cancellationToken = default)
        {

            return _pipeline.SendRequest<NoResultResponse>(CreateRequest(geneset, gene, RequestMethod.Delete),
                cancellationToken);
        }

        public async Task<SingleResultResponse<GetGeneResponse>> GetAsync(GeneSetIdentifier geneset, Gene gene, CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<SingleResultResponse<GetGeneResponse>>(CreateRequest(geneset, gene, RequestMethod.Get), cancellationToken).ConfigureAwait(false);

        }

        public SingleResultResponse<GetGeneResponse> Get(GeneSetIdentifier geneset, Gene gene, CancellationToken cancellationToken = default)
        {
            return _pipeline.SendRequest<SingleResultResponse<GetGeneResponse>>(
                CreateRequest(geneset, gene, RequestMethod.Get), cancellationToken);

        }

        internal HttpMessage CreateUploadUriRequest(GeneSetIdentifier geneset, Gene gene, GenePart part)
        {
            var message = CreateRequest(geneset, gene, RequestMethod.Get);
            message.Request.Uri.AppendPath("/part_upload_uri/", false);
            message.Request.Uri.AppendQuery(nameof(part), part.ToString().ToLowerInvariant(), true);
            return message;
        }

        public async Task<SingleResultResponse<GenePartUploadUri>> GetGenePartUploadUriAsync(GeneSetIdentifier geneset, Gene gene, GenePart part, CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<SingleResultResponse<GenePartUploadUri>>(
                CreateUploadUriRequest(geneset, gene, part), cancellationToken).ConfigureAwait(false);

        }

        public SingleResultResponse<GenePartUploadUri> GetGenePartUploadUri(GeneSetIdentifier geneset, Gene gene, GenePart part, CancellationToken cancellationToken = default)
        {
            return _pipeline.SendRequest<SingleResultResponse<GenePartUploadUri>>(
                CreateUploadUriRequest(geneset, gene, part), cancellationToken);

        }
        
        public async Task<NoResultResponse> ConfirmGenePartUploadAsync(GeneSetIdentifier geneset, Gene gene, GenePart part, CancellationToken cancellationToken = default)
        {
            return await _pipeline.SendRequestAsync<NoResultResponse>(
                CreateConfirmUploadRequest(geneset, gene, part), cancellationToken).ConfigureAwait(false);

        }

        public NoResultResponse ConfirmGenePartUpload(GeneSetIdentifier geneset, Gene gene, GenePart part, CancellationToken cancellationToken = default)
        {
            return _pipeline.SendRequest<NoResultResponse>(
                CreateConfirmUploadRequest(geneset, gene, part), cancellationToken);

        }

        internal HttpMessage CreateConfirmUploadRequest(GeneSetIdentifier geneset, Gene gene, GenePart part)
        {
            var message = CreateRequest(geneset, gene, RequestMethod.Put);
            message.Request.Uri.AppendPath("/part_uploaded", false);
            message.Request.Uri.AppendQuery(nameof(part), part.ToString().ToLowerInvariant(), true);
            return message;
        }


        internal HttpMessage CreateNewGeneRequest(NewGeneRequestBody? body)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Post;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendPath(_version, false);
            uri.AppendPath("/genes", false);
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
        /// <param name="body"> The UpdateProjectBody to use. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public async Task<SingleResultResponse<GeneUploadResponse>> CreateAsync(NewGeneRequestBody? body = null, CancellationToken cancellationToken = default)
        {

            using var message = CreateNewGeneRequest(body);
            return await _pipeline.SendRequestAsync<SingleResultResponse<GeneUploadResponse>>(message, cancellationToken).ConfigureAwait(false);
        }


        /// <summary> Creates a organization. </summary>
        /// <param name="body"> The CreateOrganizationBody to use. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public SingleResultResponse<GeneUploadResponse> Create(NewGeneRequestBody? body = null, CancellationToken cancellationToken = default)
        {

            using var message = CreateNewGeneRequest(body);
            return _pipeline.SendRequest<SingleResultResponse<GeneUploadResponse>>(message, cancellationToken);

        }

    }
}
