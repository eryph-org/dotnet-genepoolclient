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

internal class RecycleBinTagsAsyncCollection(
    RecycleBinRestClient restClient,
    OrganizationName organizationName,
    GeneSetName? genesetName, 
    ListRecycleBinRequestOptions options)
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
                    genesetName,
                    new ContinuingRequestOptions<ListRecycleBinRequestOptions>
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
            response = restClient.List(organizationName,
                genesetName,
                new ContinuingRequestOptions<ListRecycleBinRequestOptions>
                {
                    ContinuationToken = continuationToken,
                    PageSizeHint = pageSizeHint,
                    RequestOptions = options
                }
                , cancellationToken);
        }

        return Page<GenesetTagResponse>.FromValues(
            response.Value.Values?.ToList() ?? [],
            response.Value.ContinuationToken,
            response.GetRawResponse());
    }

}