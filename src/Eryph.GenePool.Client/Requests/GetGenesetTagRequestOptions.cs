using Eryph.GenePool.Model.Requests.Genesets;

namespace Eryph.GenePool.Client.Requests;

public class GetGenesetTagRequestOptions : RequestOptions
{
    /// <summary>
    /// Expand settings
    /// </summary>
    public ExpandTag Expand { get; set; }
    public bool NoCache { get; set; }

}