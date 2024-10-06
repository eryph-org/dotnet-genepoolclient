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
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Eryph.GenePool.Client;

/// <summary>
/// Client for managing api keys.
/// </summary>
[PublicAPI]
public class ApiKeyClient
{
    private readonly ClientDiagnostics _clientDiagnostics;
    internal ApiKeyRestClient RestClient { get; }
    private readonly OrganizationName _organization;
    private readonly ApiKeyId _keyId;

    internal ApiKeyClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint, 
        OrganizationName organization,
        ApiKeyId keyId)
    {
        RestClient = new ApiKeyRestClient(clientConfiguration.ClientDiagnostics, clientConfiguration.Pipeline, endpoint,
            clientConfiguration.Version);

        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _organization = organization;
        _keyId = keyId;
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DeleteAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            await RestClient.DeleteAsync(_organization, _keyId,
                options ?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void Delete(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            RestClient.Delete(_organization, _keyId,
                options ?? new RequestOptions(),
                cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<ApiKeyResponse?> GetAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_organization, _keyId,
                
                options ?? new RequestOptions(), cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual ApiKeyResponse? Get(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_organization, _keyId,
                options ?? new RequestOptions(), cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Checks if api key exists. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual bool Exists(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            _ = RestClient.Get(_organization, _keyId,
                options ?? new RequestOptions(), cancellationToken).Value;
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

    /// <summary>Checks if api key exists. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<bool> ExistsAsync(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            await RestClient.GetAsync(_organization, _keyId,
                    options ?? new RequestOptions(), 
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

}