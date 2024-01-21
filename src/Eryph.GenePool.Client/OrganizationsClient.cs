using System;
using System.Threading;
using System.Threading.Tasks;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client;

public class OrganizationsClient
{

    private readonly ClientDiagnostics _clientDiagnostics;
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;
    internal OrganizationsRestClient RestClient { get; }
    private readonly Organization _organization;


    internal OrganizationsClient(GenePoolClientConfiguration clientConfiguration, Uri endpoint, Organization organization)
    {
        RestClient = new OrganizationsRestClient(clientConfiguration.ClientDiagnostics, clientConfiguration.Pipeline, endpoint, clientConfiguration.Version);
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _organization = organization;

    }

    public virtual GenesetClient GetGenesetClient(string geneset) =>
        GetGenesetClient(Geneset.ParseUnsafe(geneset));

    public virtual GenesetClient GetGenesetClient(Geneset geneset) =>
        new(_clientConfiguration, _endpoint, _organization, geneset);

    public virtual GenesetTagClient GetGenesetTagClient(string geneset, string tag) =>
        GetGenesetTagClient(Geneset.ParseUnsafe(geneset), new Tag(tag));

    public virtual GenesetTagClient GetGenesetTagClient(Geneset geneset, Tag tag) =>
        new(_clientConfiguration, _endpoint,
            GeneSetIdentifier.ParseUnsafe($"{_organization.Value}/{geneset}/{tag}"));


    /// <summary> Creates a new organization. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <remarks> Creates a project. </remarks>
    public virtual async Task<OrganizationRefResponse?> CreateAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationsRestClient)}.{nameof(CreateAsync)}");
        scope.Start();
        try
        {
            var body = new CreateOrganizationBody()
            {
                Id = Guid.NewGuid(),
                Name = _organization.Value,
                OrgId = orgId
            };

            return (await RestClient.CreateAsync(body, cancellationToken).ConfigureAwait(false)).Value;
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
    public virtual OrganizationRefResponse? Create(Guid orgId, CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(OrganizationsRestClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new CreateOrganizationBody()
            {
                Id = Guid.NewGuid(),
                Name = _organization.Value,
                OrgId = orgId
            };
            return RestClient.Create(body, cancellationToken).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a project. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Delete");
        scope.Start();
        try
        {
            await RestClient.DeleteAsync(_organization, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Deletes a project. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void Delete(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Delete");
        scope.Start();
        try
        {
            RestClient.Delete(_organization, cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<OrganizationResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Get");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_organization, cancellationToken).ConfigureAwait(false)).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual OrganizationResponse? Get(CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Get");
        scope.Start();
        try
        {
            return RestClient.Get(_organization, cancellationToken).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public virtual async Task<OrganizationRefResponse?> RenameAsync(string newName, CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Update");
        scope.Start();
        try
        {
            var body = new UpdateOrganizationBody()
            {
                Name = newName
            };
            return (await RestClient.UpdateAsync(_organization, body, cancellationToken).ConfigureAwait(false)).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }


    public virtual OrganizationRefResponse? Rename(string newName, CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope("ProjectsClient.Update");
        scope.Start();
        try
        {
            var body = new UpdateOrganizationBody()
            {
                Name = newName
            };
            return RestClient.Update(_organization, body, cancellationToken).Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

}