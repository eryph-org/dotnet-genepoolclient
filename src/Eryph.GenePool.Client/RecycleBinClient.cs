using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Internal.AsyncCollections;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model.Responses;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client;

/// <summary>
/// Client for managing organizations.
/// </summary>
[PublicAPI]
public class RecycleBinClient
{
    private readonly ClientDiagnostics _clientDiagnostics;
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;
    internal RecycleBinRestClient RestClient { get; }
    private readonly OrganizationName _organization;


    internal RecycleBinClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint,
        OrganizationName organization)
    {
        RestClient = new RecycleBinRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _organization = organization;

    }


    /// <summary>Permanently deletes geneset tags that are in recycle bin.</summary>
    /// <param name="genesets">List of geneset tags to be deleted.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DestroyTagsAsync(GeneSetIdentifier[] genesets,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(DestroyTags)}");
        scope.Start();
        try
        {
            await RestClient.DestroyTagsAsync(
                _organization,
                genesets, cancellationToken).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>Permanently deletes geneset tags that are in recycle bin.</summary>
    /// <param name="genesets">List of geneset tags to be deleted.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void DestroyTags(GeneSetIdentifier[] genesets,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(DestroyTags)}");
        scope.Start();
        try
        {
            RestClient.DestroyTags(
                _organization,
                genesets, cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>Restores geneset tags from recycle bin.</summary>
    /// <param name="genesets">List of geneset tags to be restored.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<IEnumerable<GenesetTagResponse>?> RestoreTagsAsync(GeneSetIdentifier[] genesets,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(RestoreTags)}");
        scope.Start();
        try
        {
            return (await RestClient.RestoreTagsAsync(
                _organization,
                genesets, cancellationToken).ConfigureAwait(false)).Value.Values;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>Restores geneset tags from recycle bin.</summary>
    /// <param name="genesets">List of geneset tags to be restored.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual IEnumerable<GenesetTagResponse>? RestoreTags(GeneSetIdentifier[] genesets,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(RestoreTags)}");
        scope.Start();
        try
        {
            return RestClient.RestoreTags(
                _organization,
                genesets, cancellationToken).Value.Values;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>Gets the recycle bin content for an organization or geneset.</summary>
    /// <param name="geneSetName">Optional name of geneset to query only for given geneset.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual AsyncPageable<GenesetTagResponse> ListAsync(GeneSetName? geneSetName = default,
     CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(List)}");
        scope.Start();
        try
        {
            return new RecycleBinTagsAsyncCollection(RestClient, _organization, geneSetName).ToAsyncCollection(cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>Gets the recycle bin content for an organization or geneset.</summary>
    /// <param name="geneSetName">Optional name of geneset to query only for given geneset.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual Pageable<GenesetTagResponse> List(GeneSetName? geneSetName = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(RecycleBinClient)}.{nameof(ListAsync)}");
        scope.Start();
        try
        {
            return new RecycleBinTagsAsyncCollection(RestClient, _organization, geneSetName)
                .ToSyncCollection(cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

}