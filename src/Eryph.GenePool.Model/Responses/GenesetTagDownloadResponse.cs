using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetTagDownloadResponse : GenesetTagResponse
{
    [JsonConstructor]
    public GenesetTagDownloadResponse(string Name, GenesetResponse Geneset, string Tag, 
        GenesetTagManifestData Manifest,
        GetGeneDownloadResponse[] Genes)
        : base(Name, Geneset, Tag, null, null, Manifest, null, 
            null
            , null, null, null)
    {
        this.Genes = Genes;
    }

    [JsonPropertyName("genes")]
    public GetGeneDownloadResponse[] Genes { get; init; }

    public void Deconstruct(out string name, out GenesetResponse geneset, out string tag,
        out GenesetTagManifestData manifest, out GetGeneDownloadResponse[] genes)
    {
        name = Name;
        geneset = Geneset;
        tag = Tag;
        manifest = Manifest;
        genes = Genes;
    }
}