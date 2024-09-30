using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.RestClients;

internal class GenesetRestClient
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
    public GenesetRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
    {
        ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _endpoint = endpoint;
        _version = version.ToString().ToLowerInvariant();
    }

    internal HttpMessage CreateRequest(OrganizationName organization, GeneSetName geneset, RequestMethod method,
        string? path = null)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genesets/", false);
        uri.AppendPath(organization.Value, false);
        uri.AppendPath("/", false);
        uri.AppendPath(geneset.Value, false);

        if (!string.IsNullOrWhiteSpace(path))
        {
            uri.AppendPath($"/{path}/", false);
        }
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    public async Task<Response<NoResultResponse>> DeleteAsync(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(organization, geneset, RequestMethod.Delete), cancellationToken).ConfigureAwait(false);
    }

    public Response<NoResultResponse> Delete(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {

        return _pipeline.SendRequest<NoResultResponse>(CreateRequest(organization, geneset, RequestMethod.Delete),
            cancellationToken);
    }

    public async Task<Response<SingleResultResponse<GenesetResponse>>> GetAsync(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetResponse>>(CreateRequest(organization,geneset, RequestMethod.Get), cancellationToken).ConfigureAwait(false);

    }

    public Response<SingleResultResponse<GenesetResponse>> Get(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GenesetResponse>>(
            CreateRequest(organization, geneset, RequestMethod.Get), cancellationToken);

    }

    public async Task<Response<SingleResultResponse<GenesetDescriptionResponse>>> GetDescriptionAsync(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetDescriptionResponse>>(
            CreateRequest(organization, geneset, RequestMethod.Get, "description"), cancellationToken).ConfigureAwait(false);

    }

    public Response<SingleResultResponse<GenesetDescriptionResponse>> GetDescription(OrganizationName organization, GeneSetName geneset, CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GenesetDescriptionResponse>>(
            CreateRequest(organization, geneset, RequestMethod.Get, "description"), cancellationToken);

    }


    internal HttpMessage CreateNewGenesetRequest(NewGenesetRequestBody body)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Post;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genesets", false);
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
    public async Task<Response<SingleResultResponse<GenesetRefResponse>>> CreateAsync(NewGenesetRequestBody body, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGenesetRequest(body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a organization. </summary>
    /// <param name="body"> The CreateOrganizationBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public Response<SingleResultResponse<GenesetRefResponse>> Create(NewGenesetRequestBody body, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGenesetRequest(body);
        return _pipeline.SendRequest<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken);

    }


    internal HttpMessage CreateUpdateRequest(OrganizationName organization, GeneSetName geneset, GenesetUpdateRequestBody body)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Put;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genesets/", false);
        uri.AppendPath(organization.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(geneset.Value, true);
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
    public async Task<Response<SingleResultResponse<GenesetRefResponse>>> UpdateAsync(OrganizationName organization, GeneSetName geneset, GenesetUpdateRequestBody body, CancellationToken cancellationToken = default)
    {

        using var message = CreateUpdateRequest(organization, geneset, body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a organization. </summary>
    /// <param name="body"> The CreateOrganizationBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public Response<SingleResultResponse<GenesetRefResponse>> Update(OrganizationName organization, GeneSetName geneset, GenesetUpdateRequestBody body, CancellationToken cancellationToken = default)
    {

        using var message = CreateUpdateRequest(organization, geneset, body);
        return _pipeline.SendRequest<SingleResultResponse<GenesetRefResponse>>(message, cancellationToken);

    }
}