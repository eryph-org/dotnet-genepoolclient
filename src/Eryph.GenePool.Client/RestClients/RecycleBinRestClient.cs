﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.RestClients;

internal class RecycleBinRestClient
{
    private readonly HttpPipeline _pipeline;
    private readonly Uri _endpoint;
    private readonly string _version;

    /// <summary> The ClientDiagnostics is used to provide tracing support for the client library. </summary>
    internal ClientDiagnostics ClientDiagnostics { get; }

    /// <summary> Initializes a new instance of RecycleBinRestClient. </summary>
    /// <param name="clientDiagnostics"> The handler for diagnostic messaging in the client. </param>
    /// <param name="pipeline"> The HTTP pipeline for sending and receiving REST requests and responses. </param>
    /// <param name="endpoint"> server parameter. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="clientDiagnostics"/> or <paramref name="pipeline"/> is null. </exception>
    public RecycleBinRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
    {
        ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _endpoint = endpoint;
        _version = version.ToString().ToLowerInvariant();
    }

    internal HttpMessage CreateGenesetRequest(OrganizationName orgName, GeneSetName geneset, string path, RequestMethod method)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/recyclebin/", false);
        uri.AppendPath(orgName.Value, false);
        uri.AppendPath($"/{path}/", false);
        uri.AppendPath(geneset.Value, false);

        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    internal HttpMessage CreateOrgRequest(OrganizationName orgName, RequestMethod method, string path = "")
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/recyclebin/", false);
        uri.AppendPath(orgName.Value, false);
        if(!string.IsNullOrWhiteSpace(path))
            uri.AppendPath($"/{path}/", false);

        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    public async Task<Response<NoResultResponse>> DestroyTagsAsync(
        OrganizationName orgName, GeneSetIdentifier[] genesets,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    { 
        return await _pipeline.SendRequestAsync<NoResultResponse>(
            CreateDestroyTagsMessage(orgName, genesets),
            options,
            cancellationToken).ConfigureAwait(false);
    }

    public Response<NoResultResponse> DestroyTags(OrganizationName orgName, GeneSetIdentifier[] genesets,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<NoResultResponse>(
            CreateDestroyTagsMessage(orgName, genesets),options, cancellationToken);
    }

    private HttpMessage CreateDestroyTagsMessage(OrganizationName orgName, GeneSetIdentifier[] genesets)
    {
        var message = CreateOrgRequest(orgName, RequestMethod.Post, "destroy-tags");
        var content = new Utf8JsonRequestContent();
        content.JsonWriter.WriteObjectValue(genesets.Select(x=>$"{x.GeneSet.Value}/{x.Tag.Value}").ToArray());
        message.Request.Content = content;
        return message;
    }

    public async Task<Response<ListResultResponse<GenesetTagResponse>>> RestoreTagsAsync(
        OrganizationName orgName, GeneSetIdentifier[] genesets,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.SendRequestAsync<ListResultResponse<GenesetTagResponse>>(
            CreateRestoreTagsMessage(orgName, genesets), options, cancellationToken).ConfigureAwait(false);
    }

    public Response<ListResultResponse<GenesetTagResponse>> RestoreTags(OrganizationName orgName,
        GeneSetIdentifier[] genesetAndTags, RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<ListResultResponse<GenesetTagResponse>>(
            CreateRestoreTagsMessage(orgName, genesetAndTags), options, cancellationToken);
    }

    private HttpMessage CreateRestoreTagsMessage(OrganizationName orgName, GeneSetIdentifier[] genesets)
    {
        var message = CreateOrgRequest(orgName, RequestMethod.Post, "restore-tags");
        var content = new Utf8JsonRequestContent();
        content.JsonWriter.WriteObjectValue(genesets.Select(x => $"{x.GeneSet.Value}/{x.Tag.Value}").ToArray());
        message.Request.Content = content;
        return message;
    }

    public async Task<Response<PagedResultResponse<GenesetTagResponse>>> ListAsync(OrganizationName orgName,
        GeneSetName? genesetName,
        ContinuingRequestOptions<ListRecycleBinRequestOptions> options,
        CancellationToken cancellationToken = default)
    {

        return await _pipeline.SendRequestAsync<PagedResultResponse<GenesetTagResponse>>(
            CreateListRequest(orgName, genesetName, options),options.RequestOptions, cancellationToken).ConfigureAwait(false);

    }

    public Response<PagedResultResponse<GenesetTagResponse>> List(OrganizationName orgName,
        GeneSetName? genesetName,
        ContinuingRequestOptions<ListRecycleBinRequestOptions> options,
        CancellationToken cancellationToken = default)
    {
        return _pipeline.SendRequest<PagedResultResponse<GenesetTagResponse>>(
            CreateListRequest(orgName, genesetName, options), options.RequestOptions, cancellationToken);

    }


    private HttpMessage CreateListRequest(OrganizationName orgName, GeneSetName? genesetName, 
        ContinuingRequestOptions<ListRecycleBinRequestOptions> options)
    {
        var message = genesetName == default
            ? CreateOrgRequest(orgName, RequestMethod.Get, "tags")
            : CreateGenesetRequest(orgName, genesetName, "tags", RequestMethod.Get);

        if(!string.IsNullOrWhiteSpace(options.ContinuationToken))
            message.Request.Uri.AppendQuery("continuation_token", options.ContinuationToken);

        if (options.PageSizeHint.HasValue)
            message.Request.Uri.AppendQuery("page_size", options.PageSizeHint.Value.ToString());

        return message;
    }
    

}