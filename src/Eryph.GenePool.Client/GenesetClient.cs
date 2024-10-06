using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests;
using Eryph.GenePool.Model.Requests.Genesets;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client;

public class GenesetClient
{

    private readonly ClientDiagnostics _clientDiagnostics;
    internal GenesetRestClient RestClient { get; }
    internal UploadClient UploadClient { get; }
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;
    private readonly OrganizationName _organization;
    private readonly GeneSetName _geneset;

    internal GenesetClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint, 
        OrganizationName organization,
        GeneSetName geneset)
    {
        RestClient = new GenesetRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        UploadClient = new UploadClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.UploadPipeline);

        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
        _organization = organization;
        _geneset = geneset;
    }

    public virtual GenesetTagClient GetGenesetTagClient(string tag) =>
        new(_clientConfiguration, _endpoint,
            GeneSetIdentifier.New($"{_organization.Value}/{_geneset.Value}/{tag}"));

    public virtual GenesetTagClient GetGenesetTagClient(TagName tag) =>
        GetGenesetTagClient(tag.Value);


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
            await RestClient.DeleteAsync(_organization, _geneset,
                options ?? new RequestOptions(),
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
            RestClient.Delete(_organization, _geneset,
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
    public virtual async Task<GenesetResponse?> GetAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_organization, _geneset,
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
    public virtual GenesetResponse? Get(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_organization, _geneset,
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
    public virtual async Task<GenesetDescriptionResponse?> GetDescriptionAsync(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetDescriptionAsync(_organization, _geneset,
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
    public virtual GenesetDescriptionResponse? GetDescription(
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.GetDescription(_organization, _geneset,
                options ?? new RequestOptions(), cancellationToken).Value.Value;
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
        RequestOptions? options = default, 
            CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            _ = RestClient.Get(_organization, _geneset,
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

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<bool> ExistsAsync(
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            await RestClient.GetAsync(_organization, _geneset,
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

    public virtual async Task<GenesetRefResponse?> CreateAsync(bool isPublic,
        string? shortDescription = default,
        string? description = default,
        string? descriptionMarkdown = default,
        IDictionary<string,string>? metadata = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GenesetClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new NewGenesetRequestBody()
            {
                Geneset = $"{_organization}/{_geneset}",
                Public = isPublic,
                ShortDescription = shortDescription,
                Description = description,
                DescriptionMarkdown = descriptionMarkdown,
                Metadata = metadata
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

    public virtual async Task<GenesetRefResponse?> UpdateAsync(
        bool? isPublic = default,
        string? shortDescription = default,
        string? description = default,
        string? descriptionMarkdown = default,
        IDictionary<string, string>? metadata = default,
        string? etag = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GenesetClient)}.{nameof(Update)}");
        scope.Start();
        try
        {
            var body = new GenesetUpdateRequestBody()
            {

                Public = isPublic,
                ShortDescription = shortDescription,
                Description = description,
                DescriptionMarkdown = descriptionMarkdown,
                Metadata = metadata,
                ETag = etag
            };

            return (await RestClient.UpdateAsync(_organization, _geneset, body,
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
    public virtual GenesetRefResponse? Update(
        bool? isPublic = default,
        string? shortDescription = default,
        string? description = default,
        string? descriptionMarkdown = default,
        IDictionary<string, string>? metadata = default,
        string? etag = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Update)}");
        scope.Start();
        try
        {
            var body = new GenesetUpdateRequestBody()
            {
                Public = isPublic,
                ShortDescription = shortDescription,
                Description = description,
                DescriptionMarkdown = descriptionMarkdown,
                Metadata = metadata,
                ETag = etag
            };
            return RestClient.Update(_organization, _geneset, body,
                options ?? new RequestOptions(),
                cancellationToken).Value.Value;
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
    public virtual GenesetRefResponse? Create(bool isPublic,
        string? shortDescription = default,
        string? description = default,
        string? descriptionMarkdown = default,
        IDictionary<string, string>? metadata = default,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new NewGenesetRequestBody()
            {
                Geneset = $"{_organization}/{_geneset}",
                Public = isPublic,
                ShortDescription = shortDescription,
                Description = description,
                DescriptionMarkdown = descriptionMarkdown,
                Metadata = metadata
            };
            return RestClient.Create(body,
                options?? new RequestOptions(),
                cancellationToken).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

}