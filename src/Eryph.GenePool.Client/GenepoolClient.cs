using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Credentials;
using Eryph.GenePool.Model;
using Eryph.GenePool.Model.Responses;
using System.Text;
using Eryph.ConfigModel;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client
{
    /// <summary>
    /// The GenePoolClient provides methods to interact with a gene pool backend. 
    /// </summary>
    [PublicAPI]
    public class GenePoolClient
    {

        /// <summary>
        /// Gets the gene pools <see cref="Uri"/> endpoint.
        /// </summary>
        public virtual Uri Uri { get; }

        /// <summary>
        /// <see cref="GenePoolClientConfiguration"/>.
        /// </summary>
        private readonly GenePoolClientConfiguration _clientConfiguration;


        /// <summary>
        /// Creates a new instance of the <see cref="GenePoolClient"/>.
        /// </summary>
        /// <param name="options">Client options to use</param>
        /// <param name="uploadOptions">Upload options to use</param>
        public GenePoolClient(GenePoolClientOptions? options = default, UploadClientOptions? uploadOptions = default)
            : this(default, options ?? new GenePoolClientOptions(), 
                uploadOptions ?? new UploadClientOptions(), default, default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenePoolClient"/>.
        /// </summary>
        /// <param name="endpoint">Endpoint uri of gene pool api</param>
        /// <param name="options">Client options to use</param>
        /// <param name="uploadOptions">Upload options to use</param>
        public GenePoolClient(Uri? endpoint, GenePoolClientOptions? options = default,
            UploadClientOptions? uploadOptions = default)
            : this(endpoint, options ?? new GenePoolClientOptions(), uploadOptions ?? new UploadClientOptions(), default, default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenePoolClient"/>.
        /// </summary>
        /// <param name="tokenCredential">Token credential to use.</param>
        /// <param name="endpoint">Endpoint uri of gene pool api</param>
        /// <param name="options">Client options to use</param>
        /// <param name="uploadOptions">Upload options to use</param>
        public GenePoolClient(Uri? endpoint, TokenCredential tokenCredential, 
            GenePoolClientOptions? options = default, UploadClientOptions? uploadOptions = default)
            : this(endpoint, options ?? new GenePoolClientOptions(),
                uploadOptions ?? new UploadClientOptions(), tokenCredential, default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenePoolClient"/>.
        /// </summary>
        /// <param name="apiKeyCredential">Api Key credential to use.</param>
        /// <param name="endpoint">Endpoint uri of gene pool api</param>
        /// <param name="options">Client options to use</param>
        /// <param name="uploadOptions">Upload options to use</param>
        public GenePoolClient(Uri? endpoint, ApiKeyCredential apiKeyCredential,
            GenePoolClientOptions? options = default, UploadClientOptions? uploadOptions = default)
            : this(endpoint, options ?? new GenePoolClientOptions(),
                uploadOptions ?? new UploadClientOptions(), default, apiKeyCredential)
        {
        }

        internal GenePoolClient(
            Uri? endpoint,
            GenePoolClientOptions options, UploadClientOptions uploadOptions, TokenCredential? tokenCredential, ApiKeyCredential? apiKeyCredential)
            : this(endpoint ?? options.Endpoint,
                new GenePoolClientConfiguration(
                    pipeline: options.Build(tokenCredential, apiKeyCredential),
                    uploadPipeline: uploadOptions.Build(),
                    uploadOptions,
                    tokenCredential: tokenCredential,
                    apiKeyCredential: apiKeyCredential,
                    clientDiagnostics: new ClientDiagnostics(options),
                    version: options.Version))
        {
        }

        internal GenePoolClient(
            Uri endpoint,
            GenePoolClientConfiguration clientConfiguration)
        {
            Argument.AssertNotNull(endpoint, nameof(endpoint));
            Uri = endpoint;
            _clientConfiguration = clientConfiguration;
        }

        /// <summary>
        /// Create a new <see cref="OrganizationClient"/> for the given organization.
        /// </summary>
        /// <param name="organization">The organization as string.</param>
        /// <returns><see cref="OrganizationClient"/></returns>
        public virtual OrganizationClient GetOrganizationClient(string organization) =>
            GetOrganizationClient(OrganizationName.New(organization));

        /// <summary>
        /// Create a new <see cref="OrganizationClient"/> for the given organization.
        /// </summary>
        /// <param name="organization">The organization as <see cref="Organization"/>.</param>
        /// <returns><see cref="OrganizationClient"/></returns>
        public virtual OrganizationClient GetOrganizationClient(OrganizationName organization) =>
            new(_clientConfiguration, Uri, organization);

        /// <summary>
        /// Create a new <see cref="ApiKeyClient"/> for the given organization and api key.
        /// </summary>
        /// <param name="organization">The organization as string</param>
        /// <param name="keyId">The id of the api key</param>
        /// <returns><see cref="ApiKeyClient"/></returns>
        public virtual ApiKeyClient GetApiKeyClient(string organization, string keyId) =>
            new(_clientConfiguration, Uri, OrganizationName.New(organization), ApiKeyId.New(keyId));

        /// <summary>
        /// Create a new <see cref="ApiKeyClient"/> for the given organization and api key.
        /// </summary>
        /// <param name="organization">The organization as <see cref="Organization"/></param>
        /// <param name="keyId">The id of the api key as <see cref="ApiKeyId"/></param>
        /// <returns><see cref="ApiKeyClient"/></returns>
        public virtual ApiKeyClient GetApiKeyClient(OrganizationName organization, ApiKeyId keyId) =>
            new(_clientConfiguration, Uri, organization, keyId);

        /// <summary>
        /// Create a new <see cref="GeneClient"/> for the given geneset and gene.
        /// </summary>
        /// <param name="geneset">Geneset as string</param>
        /// <param name="gene">Gene as string</param>
        /// <returns><see cref="GeneClient"/></returns>
        public virtual GeneClient GetGeneClient(string geneset, string gene) =>
            GetGeneClient(GeneSetIdentifier.New(geneset), Gene.New(gene));

        /// <summary>
        /// Create a new <see cref="GeneClient"/> for the given geneset and gene.
        /// </summary>
        /// <param name="geneset">Geneset as <see cref="GeneSetIdentifier"/></param>
        /// <param name="gene">Gene as <see cref="Gene"/></param>
        /// <returns><see cref="GeneClient"/></returns>
        public virtual GeneClient GetGeneClient(GeneSetIdentifier geneset, Gene gene) =>
            new(_clientConfiguration, Uri, geneset, gene);

        /// <summary>
        /// Create a new <see cref="GenesetClient"/> for the given organization and geneset.
        /// </summary>
        /// <param name="organization">The organization as string.</param>
        /// <param name="geneset">The geneset name as string (without organization part).</param>
        /// <returns><see cref="GenesetClient"/></returns>
        public virtual GenesetClient GetGenesetClient(string organization, string geneset) =>
            GetGenesetClient(OrganizationName.New(organization), GeneSetName.New(geneset) );

        /// <summary>
        /// Create a new <see cref="GenesetClient"/> for the given organization and geneset.
        /// </summary>
        /// <param name="organization">The organization as <see cref="Organization"/>.</param>
        /// <param name="geneset">The geneset name as <see cref="Geneset"/></param>
        /// <returns><see cref="GenesetClient"/></returns>
        public virtual GenesetClient GetGenesetClient(OrganizationName organization, GeneSetName geneset) =>
            new(_clientConfiguration, Uri, organization, geneset);

        /// <summary>
        /// Create a new <see cref="GenesetTagClient"/> for the given organization, geneset and tag.
        /// </summary>
        /// <param name="organization">The organization as string</param>
        /// <param name="geneset">The geneset as string (without organization part)</param>
        /// <param name="tag">The geneset tag as string</param>
        /// <returns><see cref="GenesetTagClient"/></returns>
        public virtual GenesetTagClient GetGenesetTagClient(string organization, string geneset, string tag) =>
            GetGenesetTagClient($"{organization}/{geneset}/{tag}");

        /// <summary>
        /// Create a new <see cref="GenesetTagClient"/> for the given organization, geneset and tag.
        /// </summary>
        /// <param name="genesetName">The geneset identifier as string.</param>
        public virtual GenesetTagClient GetGenesetTagClient(string genesetName) =>
            GetGenesetTagClient(GeneSetIdentifier.New(genesetName));

        /// <summary>
        /// Create a new <see cref="GenesetTagClient"/> for the given organization, geneset and tag.
        /// </summary>
        /// <param name="geneset">The geneset identifier as <see cref="GeneSetIdentifier"/>.</param>
        public virtual GenesetTagClient GetGenesetTagClient(GeneSetIdentifier geneset) =>
            new(_clientConfiguration, Uri, geneset);

        /// <summary>
        /// Creates a new gene from a path and uploads it to the gene pool.
        /// </summary>
        /// <param name="geneset">Geneset for the new gene</param>
        /// <param name="path">Path of gene</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the operation</param>
        /// <param name="timeout">Timeout for waiting tasks while checking if gene is processed on backend.</param>
        /// <param name="progress">Progress reporting</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public virtual async Task<GetGeneResponse> CreateGeneFromPathAsync(string geneset, string path,
            CancellationToken cancellationToken = default, TimeSpan timeout = default,
            IProgress<GeneUploadProgress>? progress = default)
        {
            var manifestPath = Path.Combine(path, "gene.json");
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("Manifest file not found.", manifestPath);
            var manifestContent = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);

            var manifest = JsonSerializer.Deserialize<GeneManifestData>(manifestContent, GeneModelDefaults.SerializerOptions);
            if (manifest == null)
                throw new InvalidDataException("Manifest file is invalid.");

            var hashBaseJson = JsonSerializer.Serialize(manifest, GeneModelDefaults.SerializerOptions);
            var hashBytes = Encoding.UTF8.GetBytes(hashBaseJson);
            var manifestHash = GetHashString(System.Security.Cryptography.SHA256.Create().ComputeHash(hashBytes));

            var hashFromPath = Path.GetFileName(path);
            if (hashFromPath != manifestHash)
                throw new InvalidDataException("Hash of manifest file don't match path name. You may have to pack gene again.");

            var gene = Gene.New(manifestHash);

            var identifier = GeneSetIdentifier.New(geneset);

            var geneClient = new GeneClient(_clientConfiguration, Uri, identifier, gene);

            return await geneClient.UploadGeneFromPathAsync(
                path, manifest, cancellationToken, timeout, progress).ConfigureAwait(false);
        }

        private static string GetHashString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

        }
    }

}
