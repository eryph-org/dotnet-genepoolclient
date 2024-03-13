using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.RestClients;

internal class GenesetTagRestClient
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
    public GenesetTagRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
    {
        ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _endpoint = endpoint;
        _version = version.ToString().ToLowerInvariant();
    }

    internal HttpMessage CreateRequest(GeneSetIdentifier identifier, RequestMethod method, string path = "tag")
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genesets/", false);
        uri.AppendPath(identifier.Organization.Value, false);
        uri.AppendPath("/", false);
        uri.AppendPath(identifier.GeneSet.Value, false);
        uri.AppendPath($"/{path}/", false);
        uri.AppendPath(identifier.Tag.Value, false);
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    public async Task<NoResultResponse> DeleteAsync(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(geneset, RequestMethod.Delete), cancellationToken).ConfigureAwait(false);
    }

    public NoResultResponse Delete(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {

        return _pipeline.SendRequest<NoResultResponse>(CreateRequest(geneset, RequestMethod.Delete),
            cancellationToken);
    }

    public async Task<SingleResultResponse<GenesetTagResponse>> GetAsync(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetTagResponse>>(CreateRequest(geneset, RequestMethod.Get), cancellationToken).ConfigureAwait(false);

    }

    public SingleResultResponse<GenesetTagResponse> Get(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GenesetTagResponse>>(
            CreateRequest(geneset, RequestMethod.Get), cancellationToken);

    }

    public async Task<SingleResultResponse<GenesetTagDownloadResponse>> GetForDownloadAsync(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetTagDownloadResponse>>(
            CreateRequest(geneset, RequestMethod.Get, path: "download"), cancellationToken).ConfigureAwait(false);

    }

    public SingleResultResponse<GenesetTagDownloadResponse> GetForDownload(GeneSetIdentifier geneset, CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GenesetTagDownloadResponse>>(
            CreateRequest(geneset, RequestMethod.Get), cancellationToken);

    }

    internal HttpMessage CreateNewGenesetTagRequest(GenesetTagManifestData body)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Post;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genesets/tag", false);
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        request.Headers.Add("Content-Type", "application/json");
        var content = new Utf8JsonRequestContent();
        content.JsonWriter.WriteObjectValue(body);
        request.Content = content;

        return message;
    }

    /// <summary> Creates a organization. </summary>
    /// <param name="body"> The UpdateProjectBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public async Task<SingleResultResponse<GenesetRefResponse>> CreateAsync(GenesetTagManifestData body, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGenesetTagRequest(body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a organization. </summary>
    /// <param name="body"> The CreateOrganizationBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public SingleResultResponse<GenesetRefResponse> Create(GenesetTagManifestData body, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGenesetTagRequest(body);
        return _pipeline.SendRequest<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken);

    }



}