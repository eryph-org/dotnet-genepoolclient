﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests.ApiKeys;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.RestClients;

internal class ApiKeyRestClient
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
    public ApiKeyRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
    {
        ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _endpoint = endpoint;
        _version = version.ToString().ToLowerInvariant();
    }

    internal HttpMessage CreateRequest(OrganizationName organization, ApiKeyId keyId, RequestMethod method)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        // ReSharper disable once StringLiteralTypo
        uri.AppendPath("/apikeys/", false);
        uri.AppendPath(organization.Value, true);
        uri.AppendPath("/", false);
        uri.AppendPath(keyId.Value, true);
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="organization"> The organization of the api key</param>
    /// <param name="keyId">The key id of the api key</param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public async Task<Response<NoResultResponse>> DeleteAsync(OrganizationName organization, ApiKeyId keyId,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        if (organization == null)
            throw new ArgumentNullException(nameof(organization));


        return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(organization, keyId, 
            RequestMethod.Delete), options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="organization"> The organization of the api key</param>
    /// <param name="keyId">The key id of the api key</param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public Response<NoResultResponse> Delete(OrganizationName organization, ApiKeyId keyId,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        return _pipeline.SendRequest<NoResultResponse>(CreateRequest(organization, keyId, RequestMethod.Delete),
            options,
            cancellationToken);
    }

    /// <summary> Get a api key. </summary>
    /// <param name="organization"> The organization of the api key</param>
    /// <param name="keyId">The key id of the api key</param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public async Task<Response<SingleResultResponse<ApiKeyResponse>>> GetAsync(OrganizationName organization,
        ApiKeyId keyId,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }
        if (keyId == null)
        {
            throw new ArgumentNullException(nameof(keyId));
        }

        return await _pipeline.SendRequestAsync<SingleResultResponse<ApiKeyResponse>>(
            CreateRequest(organization, keyId, RequestMethod.Get), 
            options,
            cancellationToken).ConfigureAwait(false);

    }

    /// <summary> Get a organization. </summary>
    /// <param name="organization"> The organization of the api key</param>
    /// <param name="keyId">The key id of the api key</param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public Response<SingleResultResponse<ApiKeyResponse>> Get(OrganizationName organization, ApiKeyId keyId,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (keyId == null)
        {
            throw new ArgumentNullException(nameof(keyId));
        }

        return _pipeline.SendRequest<SingleResultResponse<ApiKeyResponse>>(
            CreateRequest(organization, keyId,  RequestMethod.Get),
            options,
            cancellationToken);

    }




    internal HttpMessage CreateNewApiKeyRequest(OrganizationName organization, CreateApiKeyBody? body)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Post;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        // ReSharper disable once StringLiteralTypo
        uri.AppendPath("/apikeys/", false);
        uri.AppendPath(organization.Value, false);
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

    /// <summary> Creates a api key. </summary>
    /// <param name="organization">The organization where to create the api key</param>
    /// <param name="body"> The CreateApiKeyBody to use. </param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public async Task<Response<SingleResultResponse<ApiKeySecretResponse>>> CreateAsync(
        OrganizationName organization,
        CreateApiKeyBody body,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {

        using var message = CreateNewApiKeyRequest(organization, body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<ApiKeySecretResponse>>(
            message, options, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a api key. </summary>
    /// <param name="organization">The organization where to create the api key</param>
    /// <param name="body"> The CreateApiKeyBody to use. </param>
    /// <param name="options">Request options</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public Response<SingleResultResponse<ApiKeySecretResponse>> Create(
        OrganizationName organization,
        CreateApiKeyBody body,
        RequestOptions options,
        CancellationToken cancellationToken = default)
    {

        using var message = CreateNewApiKeyRequest(organization,body);
        return _pipeline.SendRequest<SingleResultResponse<ApiKeySecretResponse>>(message,
            options,
            cancellationToken);

    }


}