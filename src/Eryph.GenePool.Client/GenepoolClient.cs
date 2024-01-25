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

namespace Eryph.GenePool.Client
{
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


        public GenePoolClient(GenePoolClientOptions? options = default, UploadClientOptions? uploadOptions = default)
            : this(default, options ?? new GenePoolClientOptions(), 
                uploadOptions ?? new UploadClientOptions(), default, default)
        {
        }

        public GenePoolClient(Uri? endpoint, GenePoolClientOptions? options = default,
            UploadClientOptions? uploadOptions = default)
            : this(endpoint, options ?? new GenePoolClientOptions(), uploadOptions ?? new UploadClientOptions(), default, default)
        {
        }

        public GenePoolClient(Uri? endpoint, TokenCredential tokenCredential, 
            GenePoolClientOptions? options = default, UploadClientOptions? uploadOptions = default)
            : this(endpoint, options ?? new GenePoolClientOptions(),
                uploadOptions ?? new UploadClientOptions(), tokenCredential, default)
        {
        }

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

        public virtual OrganizationsClient GetOrganizationsClient(string organization) =>
            GetOrganizationsClient(Organization.ParseUnsafe(organization));

        public virtual OrganizationsClient GetOrganizationsClient(Organization organization) =>
            new(_clientConfiguration, Uri, organization);


        public virtual GeneClient GetGeneClient(string geneset, string gene) =>
            GetGeneClient(GeneSetIdentifier.ParseUnsafe(geneset), Gene.New(gene));

        public virtual GeneClient GetGeneClient(GeneSetIdentifier geneset, Gene gene) =>
            new(_clientConfiguration, Uri, geneset, gene);


        public virtual GenesetClient GetGenesetClient(string organization, string geneset) =>
            GetGenesetClient(Organization.ParseUnsafe(organization), Geneset.ParseUnsafe(geneset) );

        public virtual GenesetClient GetGenesetClient(Organization organization, Geneset geneset) =>
            new(_clientConfiguration, Uri, organization, geneset);


        public virtual GenesetTagClient GetGenesetTagClient(string organization, string geneset, string tag) =>
            GetGenesetTagClient($"{organization}/{geneset}/{tag}");

        public virtual GenesetTagClient GetGenesetTagClient(string genesetName) =>
            GetGenesetTagClient(GeneSetIdentifier.ParseUnsafe(genesetName));

        public virtual GenesetTagClient GetGenesetTagClient(GeneSetIdentifier geneset) =>
            new(_clientConfiguration, Uri, geneset);


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

            var identifier = GeneSetIdentifier.ParseUnsafe(geneset);

            var geneClient = new GeneClient(_clientConfiguration, Uri, identifier, gene);

            return await geneClient.UploadGeneFromPathAsync(
                path, manifest, cancellationToken, timeout, progress);
        }

        static string GetHashString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

        }
    }

}
