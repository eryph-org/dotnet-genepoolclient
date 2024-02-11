using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests;
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
    /// <exception cref="ArgumentNullException"> <paramref name="clientDiagnostics"/> or <paramref name="pipeline"/> is null. </exception>
    public ApiKeyRestClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, Uri endpoint, GenePoolClientOptions.ServiceVersion version)
    {
        ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _endpoint = endpoint;
        _version = version.ToString().ToLowerInvariant();
    }

    internal HttpMessage CreateRequest(Organization organization, ApiKeyId keyId, RequestMethod method)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = method;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
        uri.AppendPath("/apikeys/", false);
        uri.AppendPath(organization.Value, true);
        uri.AppendPath("/", true);
        uri.AppendPath(keyId.Value, true);
        request.Uri = uri;
        request.Headers.Add("Accept", "application/json, text/json");
        return message;
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="organization"> The String to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public async Task<NoResultResponse> DeleteAsync(Organization organization, ApiKeyId keyId, CancellationToken cancellationToken = default)
    {
        if (organization == null)
            throw new ArgumentNullException(nameof(organization));


        return await _pipeline.SendRequestAsync<NoResultResponse>(CreateRequest(organization, keyId, RequestMethod.Delete), cancellationToken).ConfigureAwait(false);
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="organization"> The String to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public NoResultResponse Delete(Organization organization, ApiKeyId keyId, CancellationToken cancellationToken = default)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        return _pipeline.SendRequest<NoResultResponse>(CreateRequest(organization, keyId, RequestMethod.Delete),
            cancellationToken);
    }

    /// <summary> Get a apikey. </summary>
    /// <param name="organization"> The String to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public async Task<SingleResultResponse<ApiKeyResponse>> GetAsync(Organization organization,
        ApiKeyId keyId, CancellationToken cancellationToken = default)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }
        if (keyId == null)
        {
            throw new ArgumentNullException(nameof(keyId));
        }

        return await _pipeline.SendRequestAsync<SingleResultResponse<ApiKeyResponse>>(CreateRequest(organization, keyId, RequestMethod.Get), cancellationToken).ConfigureAwait(false);

    }

    /// <summary> Get a organization. </summary>
    /// <param name="organization"> The String to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <exception cref="ArgumentNullException"> <paramref name="organization"/> is null. </exception>
    public SingleResultResponse<ApiKeyResponse> Get(Organization organization, ApiKeyId keyId, CancellationToken cancellationToken = default)
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
            CreateRequest(organization, keyId,  RequestMethod.Get), cancellationToken);

    }




    internal HttpMessage CreateNewApiKeyRequest(Organization organization, CreateApiKeyBody? body)
    {
        var message = _pipeline.CreateMessage();
        var request = message.Request;
        request.Method = RequestMethod.Post;
        var uri = new RawRequestUriBuilder();
        uri.Reset(_endpoint);
        uri.AppendPath(_version, false);
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
    /// <param name="body"> The UpdateProjectBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public async Task<SingleResultResponse<ApiKeySecretResponse>> CreateAsync(
        Organization organization,
        CreateApiKeyBody? body = null, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewApiKeyRequest(organization, body);
        return await _pipeline.SendRequestAsync<SingleResultResponse<ApiKeySecretResponse>>(message, cancellationToken).ConfigureAwait(false);
    }


    /// <summary> Creates a api key. </summary>
    /// <param name="body"> The CreateOrganizationBody to use. </param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public SingleResultResponse<ApiKeySecretResponse> Create(
        Organization organization,
        CreateApiKeyBody? body = null, CancellationToken cancellationToken = default)
    {

        using var message = CreateNewApiKeyRequest(organization,body);
        return _pipeline.SendRequest<SingleResultResponse<ApiKeySecretResponse>>(message, cancellationToken);

    }


}