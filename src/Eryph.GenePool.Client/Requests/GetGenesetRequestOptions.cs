using Eryph.GenePool.Model.Requests.Genesets;

namespace Eryph.GenePool.Client.Requests;

public class GetGenesetRequestOptions : RequestOptions
{
    /// <summary>
    /// Expand settings
    /// </summary>
    public ExpandGeneset Expand { get; set; }
    public bool NoCache { get; set; }

}