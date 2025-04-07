using System;
using System.Threading;
using System.Threading.Tasks;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model.Responses;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client;

/// <summary>
/// Client for user information.
/// </summary>
[PublicAPI]
public class UserClient
{
    private readonly ClientDiagnostics _clientDiagnostics;
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;
    internal UserRestClient RestClient { get; }


    internal UserClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint)
    {
        RestClient = new UserRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _clientDiagnostics = clientConfiguration.ClientDiagnostics;

    }

    /// <summary> Get current user information </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="options">Request options</param>
    public virtual async Task<GetMeResponse?> GetAsync(GetUserRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(options ?? new GetUserRequestOptions(), cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get current user information. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="options">Request options</param>
    public virtual GetMeResponse? Get(GetUserRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(options ?? new GetUserRequestOptions(), cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }


    /// <summary> Get user search keys </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="options">Request options</param>
    public virtual async Task<GetSearchKeysResponse?> GetSearchKeysAsync(RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(GetSearchKeys)}");
        scope.Start();
        try
        {
            return (await RestClient.GetSearchKeysAsync(options ?? new RequestOptions(), cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get user search keys. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="options">Request options</param>
    public virtual GetSearchKeysResponse? GetSearchKeys(RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(GetSearchKeys)}");
        scope.Start();
        try
        {
            return RestClient.GetSearchKeys(options ?? new RequestOptions(), cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }
}