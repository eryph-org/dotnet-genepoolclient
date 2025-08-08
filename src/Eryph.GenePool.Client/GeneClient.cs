﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Requests.Genes;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client;

public class GeneClient
{
    private readonly ClientDiagnostics _clientDiagnostics;
    internal GenesRestClient RestClient { get; }
    internal UploadClient UploadClient { get; }
    private readonly GeneSetIdentifier _geneset;
    private readonly Gene _gene;
    private readonly GenePoolClientConfiguration _clientConfiguration;
    private readonly Uri _endpoint;

    internal GeneClient(
        GenePoolClientConfiguration clientConfiguration,
        Uri endpoint,
        GeneSetIdentifier geneset,
        Gene gene)
    {
        RestClient = new GenesRestClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.Pipeline,
            endpoint,
            clientConfiguration.Version);
        UploadClient = new UploadClient(
            clientConfiguration.ClientDiagnostics,
            clientConfiguration.UploadPipeline);

        _clientDiagnostics = clientConfiguration.ClientDiagnostics;
        _geneset = geneset;
        _gene = gene;
        _clientConfiguration = clientConfiguration;
        _endpoint = endpoint;
    }

    /// <summary> Creates a new gene. </summary>
    /// <param name="manifest">Gene manifest</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="gene">Gene</param>
    /// <param name="yamlContent">Original yaml content of a gene</param>
    /// <remarks> Creates a project. </remarks>
    public virtual async Task<GeneUploadResponse?> CreateAsync(
        Gene gene,
        GeneManifestData manifest,
        RequestOptions? options = default, 
        CancellationToken cancellationToken = default,
        string? yamlContent = null)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new NewGeneRequestBody
            {
                Gene = gene.Value,
                Geneset = _geneset.Value,
                Manifest = manifest,
                YamlContent = yamlContent
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
    /// <param name="manifest">Gene manifest</param>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    /// <param name="gene">Gene</param>
    /// <param name="yamlContent">Original yaml content of a gene</param>
    /// <remarks> Creates a project. </remarks>
    public virtual GeneUploadResponse? Create(Gene gene, GeneManifestData manifest,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default,
        string? yamlContent = null)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Create)}");
        scope.Start();
        try
        {
            var body = new NewGeneRequestBody
            {
                Gene = gene.Value,
                Geneset = _geneset.Value,
                Manifest = manifest,
                YamlContent = yamlContent
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
            await RestClient.DeleteAsync(_geneset, _gene,
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
            RestClient.Delete(_geneset, _gene, options?? new RequestOptions(), cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<GetGeneResponse?> GetAsync(
        GetGeneRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return (await RestClient.GetAsync(_geneset, _gene,
                options ?? new GetGeneRequestOptions(),
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
    public virtual GetGeneResponse? Get(
        GetGeneRequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            return RestClient.Get(_geneset, _gene,
                options ?? new GetGeneRequestOptions(),
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
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(Get)}");
        scope.Start();
        try
        {
            var res = RestClient.Get(_geneset, _gene,
                new GetGeneRequestOptions(options ?? new RequestOptions()),
                cancellationToken).Value;
            return true;
        }
        catch (ErrorResponseException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
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
            await RestClient.GetAsync(_geneset, _gene,
                    new GetGeneRequestOptions(options ?? new RequestOptions()),
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

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual async Task<GenePartUploadUri?> GetUploadUriAsync(string genePart,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(GetUploadUri)}");
        scope.Start();
        try
        {
            var parsedPart = GenePart.New(genePart);
            return (await RestClient.GetGenePartUploadUriAsync(_geneset, _gene, parsedPart,
                    new RequestOptions(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)).Value.Value;
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual GenePartUploadUri? GetUploadUri(string genePart,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(GetUploadUri)}");
        scope.Start();
        try
        {
            var parsedPart = GenePart.New(genePart);
            return RestClient.GetGenePartUploadUri(_geneset, _gene, parsedPart,
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
    public virtual async Task ConfirmPartUploadAsync(string genePart,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(ConfirmPartUploadAsync)}");
        scope.Start();
        try
        {
            var parsedPart = GenePart.New(genePart);
            await RestClient.ConfirmGenePartUploadAsync(_geneset, _gene, parsedPart,
                    options ?? new RequestOptions(),
                    cancellationToken)
                .ConfigureAwait(false);

        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    /// <summary> Get a projects. </summary>
    /// <param name="cancellationToken"> The cancellation token to use. </param>
    public virtual void ConfirmPartUpload(string genePart,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(ConfirmPartUploadAsync)}");
        scope.Start();
        try
        {
            var parsedPart = GenePart.New(genePart);
            RestClient.ConfirmGenePartUpload(_geneset, _gene, parsedPart,
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
    public virtual void UploadPart(GenePartUploadUri uploadUri, Stream content,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(ConfirmPartUploadAsync)}");
        scope.Start();
        try
        {
            var timeLeft = (uploadUri.Expires - DateTimeOffset.UtcNow).TotalSeconds;
            if (timeLeft < 10)
                uploadUri = GetUploadUri(uploadUri.Part,
                               options, cancellationToken) ??
                            throw new IOException("Failed to refresh upload url.");

            UploadClient.UploadPart(uploadUri.UploadUri, content, cancellationToken);
            RestClient.ConfirmGenePartUpload(_geneset, _gene, GenePart.New(uploadUri.Part),
                options ?? new RequestOptions(),
                cancellationToken);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public virtual async Task UploadPartAsync(GenePartUploadUri uploadUri, Stream content,
        RequestOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        using var scope = _clientDiagnostics.CreateScope($"{nameof(GeneClient)}.{nameof(ConfirmPartUploadAsync)}");
        scope.Start();
        try
        {
            var timeLeft = (uploadUri.Expires - DateTimeOffset.UtcNow).TotalSeconds;
            if (timeLeft < 10)
                uploadUri = await GetUploadUriAsync(uploadUri.Part, cancellationToken).ConfigureAwait(false) ??
                            throw new IOException("Failed to refresh upload url.");

            await UploadClient.UploadPartAsync(uploadUri.UploadUri, content, cancellationToken).ConfigureAwait(false);
            await RestClient.ConfirmGenePartUploadAsync(_geneset, _gene, GenePart.New(uploadUri.Part),
                options?? new RequestOptions(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            scope.Failed(e);
            throw;
        }
    }

    public bool UploadMissingParts(GetGeneResponse geneStatus,
        string genePath, 
        GenePartUploadUri[]? uploadUris = default, 
        CancellationToken cancellationToken = default, 
        IProgress<GeneUploadProgress>? progress = default)
    {
        var stagedParts = geneStatus.UploadStatus?.ConfirmedParts
            .Select(p => p.Split(':')[1]) ?? Array.Empty<string>();

        var partsRequired = (geneStatus.Manifest.Parts ?? [])
            .Select(p => p.Split(':')[1]).Except(stagedParts).ToArray();

        var uploadedPart = false;
        uploadUris ??= [];

        var totalMissingSize = 0L;
        var totalMissingCount = partsRequired.Length;
        foreach (var part in partsRequired)
        {
            var partPath = Path.Combine(genePath, $"{part}.part");
            if (!File.Exists(partPath))
                throw new FileNotFoundException($"Part file '{Path.GetFileName(partPath)}' not found.", partPath);

            totalMissingSize += new FileInfo(partPath).Length;
        }

        const int totalUploadedCount = 0;
        var totalUploadedSize = 0L;

        Parallel.ForEach(partsRequired, new ParallelOptions
        {
            MaxDegreeOfParallelism =  _clientConfiguration.UploadOptions.MaxParallelThreads
        }, part =>
        {
            try
            {
                var partPath = Path.Combine(genePath, $"{part}.part");
                if (!File.Exists(partPath))
                    throw new FileNotFoundException("Part file not found.", partPath);

                var partSize = new FileInfo(partPath).Length;
                var uploadUri = uploadUris.FirstOrDefault(x => x.Part == part) ??
                                new GenePartUploadUri(part, new Uri("http://localhost"),
                                    DateTimeOffset.MinValue);

                using var fileStream = File.OpenRead(partPath);
                Stream partStream = fileStream;
                GeneUploadProgress? progressData = null;
                var streamProgressBytes = 0L;

                if (progress != null)
                {
                    progressData = new GeneUploadProgress
                    {
                        Part = part,
                        FilePath = partPath,
                        PartSize = partSize,
                        TotalMissingSize = totalMissingSize,
                        TotalMissingCount = totalMissingCount,
                        TotalUploadedCount = totalUploadedCount,
                        TotalUploadedSize = totalUploadedSize
                    };

                    progress.Report(progressData);

                    var streamProgress = new Progress<long>();
                    streamProgress.ProgressChanged += (_, size) =>
                    {
                        streamProgressBytes += size;
                        if (streamProgressBytes >= 1024*1024)
                        {
                            Interlocked.Add(ref totalUploadedSize, streamProgressBytes);
                            streamProgressBytes = 0;
                        }

                        progressData.TotalUploadedSize = totalUploadedSize;
                        progress.Report(progressData);
                    };

                    partStream = new ProgressStream(fileStream, streamProgress);

                }


                UploadPart(uploadUri, partStream, new RequestOptions(), cancellationToken);
                uploadedPart = true;

                if (progress == null || progressData == null) return;
                Interlocked.Add(ref totalUploadedSize, streamProgressBytes);
                progressData.TotalUploadedSize = totalUploadedSize;
                progressData.Uploaded = true;
                progress.Report(progressData);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
            {
                // ignore conflicts in upload, it means that part is already uploaded and in processing
            }
        });

        return uploadedPart;
    }

    public virtual async Task<GetGeneResponse> UploadGeneFromPathAsync(string path,
    Gene gene,
    GeneManifestData manifest,
    CancellationToken cancellationToken = default, TimeSpan timeout = default,
    RequestOptions? options = default,
    IProgress<GeneUploadProgress>? progress = default)
    {
        options ??= new RequestOptions();
        var getOptions = new GetGeneRequestOptions(options);

        var geneExists = await ExistsAsync(options, cancellationToken).ConfigureAwait(false);
        var yamlContentPath = Path.Combine(path, "gene.yaml");
        string? yamlContent = null;
        if(File.Exists(yamlContentPath))
            yamlContent = await File.ReadAllTextAsync(yamlContentPath, cancellationToken).ConfigureAwait(false);

        var uploadUris = Array.Empty<GenePartUploadUri>();
        if (!geneExists)
        {
            var createResponse = await CreateAsync(gene,manifest, options, cancellationToken, yamlContent).ConfigureAwait(false);
            uploadUris = createResponse?.UploadUris ?? uploadUris;
        }

        var geneStatus = await GetAsync(getOptions, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Gene not found.");
        if (geneStatus.Available.GetValueOrDefault())
            return geneStatus;

        var uploaded = UploadMissingParts(geneStatus, path, uploadUris, cancellationToken, progress: progress);

        if (!uploaded)
            return geneStatus;

        if (timeout == default)
            timeout = TimeSpan.FromMinutes(5);

        var processingCancellation = new CancellationTokenSource(timeout);
        var processingTimeout = CancellationTokenSource.CreateLinkedTokenSource(
            processingCancellation.Token, cancellationToken);

        while (!processingTimeout.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            geneStatus = await GetAsync(getOptions, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Gene not found.");
            if (geneStatus.Available.GetValueOrDefault())
                return geneStatus;

            uploaded = UploadMissingParts(geneStatus, path, uploadUris, cancellationToken, progress: progress);
            if (uploaded)
                processingCancellation.CancelAfter(timeout);
        }

        throw new TimeoutException("Timeout while waiting for gene to become available.");


    }

}