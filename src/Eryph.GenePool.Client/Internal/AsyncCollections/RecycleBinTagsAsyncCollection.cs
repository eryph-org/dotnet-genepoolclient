using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.Internal.AsyncCollections
{
    internal class RecycleBinTagsAsyncCollection(
        RecycleBinRestClient restClient,
        OrganizationName organizationName,
        GeneSetName? genesetName)
        : CollectionEnumerator<GenesetTagResponse>
    {

        public override async ValueTask<Page<GenesetTagResponse>> GetNextPageAsync(
            string? continuationToken,
            int? pageSizeHint,
            bool async,
            CancellationToken cancellationToken)
        {
            Response<PagedResultResponse<GenesetTagResponse>> response;
            if (async)
            {
                response = await restClient.ListAsync(organizationName,
                        genesetName, continuationToken, pageSizeHint, cancellationToken)
                    .ConfigureAwait(false);

            }
            else
            {
                // ReSharper disable once MethodHasAsyncOverload
                response = restClient.List(organizationName,
                    genesetName, continuationToken, pageSizeHint, cancellationToken);
            }

            return Page<GenesetTagResponse>.FromValues(
                response.Value.Values?.ToList() ?? [],
                response.Value.ContinuationToken,
                response.GetRawResponse());
        }

    }
}
