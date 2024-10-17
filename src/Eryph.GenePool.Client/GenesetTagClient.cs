using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client;

public class GenesetTagClient
{

    private readonly ClientDiagnostics _clientDiagnostics;
    internal GenesetTagRestClient RestClient { get; }
    internal UploadClient UploadClient { get; }
    private readonly GeneSetIdentifier _identifier; 
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;

    internal GenesetTagClient(GenePoolClientConfiguration clientConfiguration, Uri endpoint,
        GeneSetIdentifier identifier)
    {
        RestClient = new GenesetTagRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        UploadClient = new UploadClient(clientConfiguration.ClientDiagnostics, clientConfiguration.UploadPipeline);

        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _identifier = identifier;
    }

    public virtual GeneClient GetGeneClient(string gene) =>
        GetGeneClient(Gene.New(gene));

    public virtual GeneClient GetGeneClient(Gene gene) =>
        new(_clientConfiguration, _endpoint, _identifier, gene);


    /// <summary> Deletes a project. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DeleteAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            await RestClient.DeleteAsync(_identifier,
                options?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a project. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void Delete(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            RestClient.Delete(_identifier,
                options ?? new RequestOptions(),
                cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<GenesetTagResponse?> GetAsync(
        GetGenesetTagRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_identifier,
                options ?? new GetGenesetTagRequestOptions(),
                cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual GenesetTagResponse? Get(
        GetGenesetTagRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_identifier,
                options ?? new GetGenesetTagRequestOptions(),
                cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public virtual async Task<GenesetTagDownloadResponse?> GetForDownloadAsync(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetForDownloadAsync(_identifier,
                options ?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual GenesetTagDownloadResponse? GetForDownload(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.GetForDownload(_identifier,
                options ?? new RequestOptions(),
                cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual bool Exists(
        GetGenesetTagRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            var res = RestClient.Get(_identifier,
                options ?? new GetGenesetTagRequestOptions(), 
                cancellationToken).Value;
            return true;
        }
        catch (ErrorResponseException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<bool> ExistsAsync(GetGenesetTagRequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            await RestClient.GetAsync(_identifier,
                    options ?? new GetGenesetTagRequestOptions(), 
                    cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (ErrorResponseException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }


    public virtual async Task<GenesetRefResponse?> CreateAsync(GenesetTagManifestData manifest,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GenesetTagClient)}.{nameof(Create)}");
        scope.Start();
        try
        {

            return (await RestClient.CreateAsync(manifest,
                options ?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Creates a new organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <remarks> Creates a project. </remarks>
    public virtual GenesetRefResponse? Create(GenesetTagManifestData manifest,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GenesetTagClient)}.{nameof(Create)}");
        scope.Start();
        try
        {

            return RestClient.Create(manifest,
                options ?? new RequestOptions(), cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }
}