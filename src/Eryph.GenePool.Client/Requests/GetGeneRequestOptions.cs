using Eryph.GenePool.Model.Requests.Genesets;
using System;
using System.Collections.Generic;
using System.Text;
using Eryph.GenePool.Model.Requests.Genes;

namespace Eryph.GenePool.Client.Requests;

public class GetGeneRequestOptions() : RequestOptions
{
    internal GetGeneRequestOptions(RequestOptions options) : this()
    {
        OnResponse = options.OnResponse;
    }

    public ExpandGene Expand { get; set; }
}
