using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client;

/// <summary>
/// Client for managing api keys.
/// </summary>
[PublicAPI]
public class ApiKeyClient
{

    private readonly ClientDiagnostics _clientDiagnostics;
    internal ApiKeyRestClient RestClient { get; }
    private readonly Organization _organization;
    private readonly ApiKeyId _keyId;

    internal ApiKeyClient(GenePoolClientConfiguration clientConfiguration, Uri endpoint, 
        Organization organization, ApiKeyId keyId)
    {
        RestClient = new ApiKeyRestClient(clientConfiguration.ClientDiagnostics, clientConfiguration.Pipeline, endpoint,
            clientConfiguration.Version);

        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _organization = organization;
        _keyId = keyId;
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            await RestClient.DeleteAsync(_organization, _keyId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void Delete(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            RestClient.Delete(_organization, _keyId, cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<ApiKeyResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_organization, _keyId, cancellationToken).ConfigureAwait(false)).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a api key. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual ApiKeyResponse? Get(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_organization, _keyId, cancellationToken).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Checks if api key exists. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual bool Exists(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            _ = RestClient.Get(_organization, _keyId, cancellationToken).Value;
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
    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(ApiKeyClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            await RestClient.GetAsync(_organization, _keyId, cancellationToken)
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