using Eryph.GenePool.Model.Requests.Genesets;

namespace Eryph.GenePool.Client.Requests;

public class ListGenesetsRequestOptions : RequestOptions
{
    public ExpandGeneset Expand { get; set; }
}