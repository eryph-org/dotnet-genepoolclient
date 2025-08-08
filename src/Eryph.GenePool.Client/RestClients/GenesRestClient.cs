﻿using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using Azure;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;
using Eryph.GenePool.Model.Requests.Genes;

namespace Eryph.GenePool.Client.RestClients;

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
        uri.AppendPath(identifier.Organization.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(identifier.GeneSet.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(identifier.Tag.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(gene.Value, true);
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    internal HttpMessage CreateGetRequest(GeneSetIdentifier identifier, Gene gene, GetGeneRequestOptions options)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Get;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/genes/", false);
        uri.AppendPath(identifier.Organization.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(identifier.GeneSet.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(identifier.Tag.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(gene.Value, true);

        uri.AddExpandOptions(options.Expand);

        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    public async Task<Response<NoResultResponse>> DeleteAsync(GeneSetIdentifier geneset, Gene gene,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(geneset, gene, 
            RequestMethod.Delete), options, cancellationToken).ConfigureAwait(false);
    }

    public Response<NoResultResponse> Delete(GeneSetIdentifier geneset, Gene gene,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {

        return _pipeline.SendRequest<NoResultResponse>(CreateRequest(geneset, gene, RequestMethod.Delete),
            options, cancellationToken);
    }

    public async Task<Response<SingleResultResponse<GetGeneResponse>>> GetAsync(GeneSetIdentifier geneset, Gene gene,
        GetGeneRequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GetGeneResponse>>(
            CreateGetRequest(geneset, gene, options),options, cancellationToken).ConfigureAwait(false);

    }

    public Response<SingleResultResponse<GetGeneResponse>> Get(GeneSetIdentifier geneset, Gene gene,
        GetGeneRequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GetGeneResponse>>(
            CreateGetRequest(geneset, gene, options), options, cancellationToken);

    }

    internal HttpMessage CreateUploadUriRequest(GeneSetIdentifier geneset, Gene gene, GenePart part)
    {
        var message = CreateRequest(geneset, gene, RequestMethod.Get);
        message.Request.Uri.AppendPath("/part_upload_uri/", false);
        message.Request.Uri.AppendQuery(nameof(part), part.ToString().ToLowerInvariant(), true);
        return message;
    }

    public async Task<Response<SingleResultResponse<GenePartUploadUri>>> GetGenePartUploadUriAsync(
        GeneSetIdentifier geneset, Gene gene, GenePart part,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<SingleResultResponse<GenePartUploadUri>>(
            CreateUploadUriRequest(geneset, gene, part), options, cancellationToken).ConfigureAwait(false);

    }

    public Response<SingleResultResponse<GenePartUploadUri>> GetGenePartUploadUri(
        GeneSetIdentifier geneset, Gene gene, GenePart part,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<SingleResultResponse<GenePartUploadUri>>(
            CreateUploadUriRequest(geneset, gene, part), options, cancellationToken);

    }
        
    public async Task<Response<NoResultResponse>> ConfirmGenePartUploadAsync(GeneSetIdentifier geneset, Gene gene,
        GenePart part, RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<NoResultResponse>(
            CreateConfirmUploadRequest(geneset, gene, part),
            options, cancellationToken).ConfigureAwait(false);

    }

    public Response<NoResultResponse> ConfirmGenePartUpload(GeneSetIdentifier geneset, Gene gene, GenePart part,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<NoResultResponse>(
            CreateConfirmUploadRequest(geneset, gene, part),
            options,
            cancellationToken);

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
    public async Task<Response<SingleResultResponse<GeneUploadResponse>>> CreateAsync(
        NewGeneRequestBody body,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGeneRequest(body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<GeneUploadResponse>>(message, 
            options, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a organization. </summary>
    /// <param name="body"> The CreateOrganizationBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public Response<SingleResultResponse<GeneUploadResponse>> Create(NewGeneRequestBody body,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {

        using var message = CreateNewGeneRequest(body);
        return _pipeline.SendRequest<SingleResultResponse<GeneUploadResponse>>(message,
            options, cancellationToken);

    }


}