using System;
using System.Threading;
using System.Threading.Tasks;
using Eryph.GenePool.Client.Internal;
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
    /// <param name="expandGenepoolOrgs">Gets the genepool organization names that the user is a member of.</param>
    /// <param name="expandIdentityOrgs">Gets details for identity organizations that the user is a member of.</param>
    public virtual async Task<GetMeResponse?> GetAsync(bool expandGenepoolOrgs= false, bool expandIdentityOrgs= false,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(
                expandGenepoolOrgs, expandIdentityOrgs, cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get current user information. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="expandGenepoolOrgs">Gets the genepool organization names that the user is a member of.</param>
    /// <param name="expandIdentityOrgs">Gets details for identity organizations that the user is a member of.</param>
    public virtual GetMeResponse? Get(
        bool expandGenepoolOrgs = false, bool expandIdentityOrgs = false,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(UserClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(
                expandGenepoolOrgs, expandIdentityOrgs, cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }
}