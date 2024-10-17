using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Eryph.ConfigModel;
using Eryph.GenePool.Client.Requests;
using Eryph.GenePool.Client.Responses;
using Eryph.GenePool.Client.RestClients;
using Eryph.GenePool.Model.Responses;

namespace Eryph.GenePool.Client.Internal.AsyncCollections;

internal class GenesetsAsyncCollection(
    OrganizationsRestClient restClient,
    OrganizationName organizationName,
    ListGenesetsRequestOptions options)
    : CollectionEnumerator<GenesetResponse>
{

    public override async ValueTask<Page<GenesetResponse>> GetNextPageAsync(
        string? continuationToken,
        int? pageSizeHint,
        bool async,
        CancellationToken cancellationToken)
    {
        Response<PagedResultResponse<GenesetResponse>> response;
        if (async)
        {
            response = await restClient.ListGenesetsAsync(organizationName,
                    new ContinuingRequestOptions<ListGenesetsRequestOptions>
                    {
                        ContinuationToken = continuationToken,
                        PageSizeHint = pageSizeHint,
                        RequestOptions = options
                    }
                    , cancellationToken)
                .ConfigureAwait(false);

        }
        else
        {
            // ReSharper disable once MethodHasAsyncOverload
            response = restClient.ListGenesets(organizationName,
                new ContinuingRequestOptions<ListGenesetsRequestOptions>
                {
                    ContinuationToken = continuationToken,
                    PageSizeHint = pageSizeHint,
                    RequestOptions = options
                }
                , cancellationToken);
        }

        return Page<GenesetResponse>.FromValues(
            response.Value.Values?.ToList() ?? [],
            response.Value.ContinuationToken,
            response.GetRawResponse());
    }

}