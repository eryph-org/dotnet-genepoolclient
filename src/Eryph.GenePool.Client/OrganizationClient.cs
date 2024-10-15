using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Internal.AsyncCollections;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests.ApiKeys;
using Eryph.GenePool.Model.Requests.Organizations;
using Eryph.GenePool.Model.Responses;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client;

/// <summary>
/// Client for managing organizations.
/// </summary>
[PublicAPI]
public class OrganizationClient
{
    private readonly ClientDiagnostics _clientDiagnostics;
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;
    internal OrganizationsRestClient RestClient { get; }
    private readonly OrganizationName _organization;


    internal OrganizationClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint,
        OrganizationName organization)
    {
        RestClient = new OrganizationsRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _organization = organization;

    }

    /// <summary>
    /// Gets a <see cref="GenesetClient"/> for the current organization and the specified geneset.
    /// </summary>
    /// <param name="geneset">The geneset as string</param>
    /// <returns></returns>
    public virtual GenesetClient GetGenesetClient(string geneset) =>
        GetGenesetClient(GeneSetName.New(geneset));

    /// <summary>
    /// Gets a <see cref="GenesetClient"/> for the current organization and the specified geneset.
    /// </summary>
    /// <param name="geneset">The geneset as <see cref="GeneSetName"/></param>
    /// <returns></returns>
    public virtual GenesetClient GetGenesetClient(GeneSetName geneset) =>
        new(_clientConfiguration, _endpoint, _organization, geneset);

    /// <summary>
    /// Gets a <see cref="GenesetTagClient"/> for the current organization and the specified geneset and tag.
    /// </summary>
    /// <param name="geneset">The geneset as <see cref="string"/></param>
    /// <param name="tag">The geneset tag as string</param>
    /// <returns></returns>
    public virtual GenesetTagClient GetGenesetTagClient(string geneset, string tag) =>
        GetGenesetTagClient(GeneSetName.New(geneset), TagName.New(tag));

    /// <summary>
    /// Gets a <see cref="GenesetTagClient"/> for the current organization and the specified geneset and tag.
    /// </summary>
    /// <param name="geneset">The geneset as <see cref="GeneSetName"/></param>
    /// <param name="tag">The geneset tag as <see cref="TagName"/></param>
    /// <returns></returns>
    public virtual GenesetTagClient GetGenesetTagClient(GeneSetName geneset, TagName tag) =>
        new(_clientConfiguration, _endpoint,
            GeneSetIdentifier.New($"{_organization.Value}/{geneset}/{tag}"));

    /// <summary>
    /// Gets a <see cref="ApiKeyClient"/> for the current organization and the specified key id.
    /// </summary>
    /// <param name="keyId">The api key id as string</param>
    /// <returns></returns>
    public virtual ApiKeyClient GetApiKeyClient(string keyId) =>
        GetApiKeyClient(ApiKeyId.New(keyId));

    /// <summary>
    /// Gets a <see cref="ApiKeyClient"/> for the current organization and the specified key id.
    /// </summary>
    /// <param name="keyId">The api key id as <see cref="ApiKeyId"/></param>
    /// <returns></returns>
    public virtual ApiKeyClient GetApiKeyClient(ApiKeyId keyId) =>
        new(_clientConfiguration, _endpoint, _organization, keyId);


    /// <summary>
    /// Gets a <see cref="RecycleBinClient"/> for the current organization.
    /// </summary>
    /// <returns></returns>
    public virtual RecycleBinClient GetRecycleBinClient() =>
        new(_clientConfiguration, _endpoint, _organization);


    /// <summary> Creates a new organization. </summary>
    /// <param name="genepoolOrgId">The unique id of the genepool organization.</param>
    /// <param name="ownerOrgId">The id of identity organization that will own the genepool org.</param>
    /// <param name="newOrgName">If specified a new identity organization will be created with given name.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <remarks> Creates a project. </remarks>
    public virtual async Task<OrganizationRefResponse?> CreateAsync(Guid genepoolOrgId,
        Guid ownerOrgId, string? newOrgName = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(CreateAsync)}");
        scope.Start();
        try
        {
            var body = new CreateOrganizationBody
            {
                Id = genepoolOrgId,
                Name = _organization.Value,
                OrgId = ownerOrgId,
                NewOrgName = newOrgName
            };

            return (await RestClient.CreateAsync(body,
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
    /// <param name="genepoolOrgId">The unique id of the genepool organization.</param>
    /// <param name="ownerOrgId">The id of identity organization that will own the genepool org.</param>
    /// <param name="newOrgName">If specified a new identity organization will be created with given name.</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <remarks> Creates a project. </remarks>
    public virtual OrganizationRefResponse? Create(Guid genepoolOrgId,
        Guid ownerOrgId, string? newOrgName = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new CreateOrganizationBody
            {
                Id = genepoolOrgId,
                Name = _organization.Value,
                OrgId = ownerOrgId,
                NewOrgName = newOrgName
            };
            return RestClient.Create(body,
                options ?? new RequestOptions(),
                cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DeleteAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            await RestClient.DeleteAsync(_organization,
                options ?? new RequestOptions(), 
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void Delete(RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Delete)}");
        scope.Start();
        try
        {
            RestClient.Delete(_organization,
                options ?? new RequestOptions(), cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<OrganizationResponse?> GetAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_organization,
                options ?? new RequestOptions(), cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual OrganizationResponse? Get(
        RequestOptions? options = default, CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_organization,
                options ?? new RequestOptions(), 
                cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    // currently not supported, will be added in future versions

    ///// <summary>
    ///// Renames the organization.
    ///// </summary>
    ///// <param name="newName">new name of the organization.</param>
    ///// <param name="etag"></param>
    ///// <param name="cancellationToken"></param>
    ///// <returns></returns>
    //public virtual async Task<OrganizationRefResponse?> RenameAsync(string newName, string? etag = null, CancellationToken cancellationToken = default)
    //{
    //    using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Rename)}");
    //    scope.Start();
    //    try
    //    {
    //        var body = new UpdateOrganizationBody()
    //        {
    //            Name = newName,
    //            ETag = etag
    //        };
    //        return (await RestClient.UpdateAsync(_organization, body, cancellationToken).ConfigureAwait(false)).Value;
    //    }
    //    catch (Exception e)
    //    {
    //        scope.Failed(e);
    //        throw;
    //    }
    //}

    ///// <summary>
    ///// Renames the organization.
    ///// </summary>
    ///// <param name="newName">New name of the organization.</param>
    ///// <param name="cancellationToken"></param>
    ///// <returns></returns>
    //public virtual OrganizationRefResponse? Rename(string newName, string? etag = null, CancellationToken cancellationToken = default)
    //{
    //    using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(Rename)}");
    //    scope.Start();
    //    try
    //    {
    //        var body = new UpdateOrganizationBody()
    //        {
    //            Name = newName,
    //            ETag = etag
    //        };
    //        return RestClient.Update(_organization, body, cancellationToken).Value;
    //    }
    //    catch (Exception e)
    //    {
    //        scope.Failed(e);
    //        throw;
    //    }
    //}

    /// <summary>
    /// Creates a new api key for the organization.
    /// </summary>
    /// <param name="name">human-readable name of the api key</param>
    /// <param name="permissions">permissions to include into the api key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<ApiKeySecretResponse?> CreateApiKeyAsync(
        string name,
        string[] permissions,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default) =>
        CreateApiKeyAsync(ApiKeyName.New(name), permissions, options, cancellationToken);

    /// <summary>
    /// Creates a new api key for the organization.
    /// </summary>
    /// <param name="name">human-readable name of the api key</param>
    /// <param name="permissions">permissions to include into the api key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ApiKeySecretResponse?> CreateApiKeyAsync(
        ApiKeyName name,
        string[] permissions,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(CreateApiKey)}");
        scope.Start();
        try
        {
            var body = new CreateApiKeyBody
            {
                Name = name.Value,
                Permissions = permissions
            };

            var apiKeyRestClient = new ApiKeyRestClient(
                _clientConfiguration.ClientDiagnostics, 
                _clientConfiguration.Pipeline, _endpoint, 
                _clientConfiguration.Version);


            return (await apiKeyRestClient.CreateAsync(_organization, body,
                options?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary>
    /// Creates a new api key for the organization.
    /// </summary>
    /// <param name="name">human-readable name of the api key</param>
    /// <param name="permissions">permissions to include into the api key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual ApiKeySecretResponse? CreateApiKey(
        string name,
        string[] permissions,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default) =>
        CreateApiKey(ApiKeyName.New(name), permissions, options, cancellationToken);

    /// <summary>
    /// Creates a new api key for the organization.
    /// </summary>
    /// <param name="name">human.readable name of the api key</param>
    /// <param name="permissions">permissions to include into the api key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual ApiKeySecretResponse? CreateApiKey(
        ApiKeyName name,
        string[] permissions,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(CreateApiKey)}");
        scope.Start();
        try
        {
            var body = new CreateApiKeyBody
            {
                Name = name.Value,
                Permissions = permissions
            };

            var apiKeyRestClient = new ApiKeyRestClient(
                _clientConfiguration.ClientDiagnostics,
                _clientConfiguration.Pipeline, _endpoint,
                _clientConfiguration.Version);


            return apiKeyRestClient.Create(_organization, body, options ?? new RequestOptions(), cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public virtual AsyncPageable<GenesetResponse> ListGenesetsAsync(
        ListGenesetsRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(ListGenesets)}");
        scope.Start();
        try
        {
            return new GenesetsAsyncCollection(RestClient, _organization,
                options ?? new ListGenesetsRequestOptions()
            ).ToAsyncCollection(cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public virtual Pageable<GenesetResponse> ListGenesets(
        ListGenesetsRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationClient)}.{nameof(ListGenesets)}");
        scope.Start();
        try
        {
            return new GenesetsAsyncCollection(RestClient, _organization,
                    options ?? new ListGenesetsRequestOptions())
                .ToSyncCollection(cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

}