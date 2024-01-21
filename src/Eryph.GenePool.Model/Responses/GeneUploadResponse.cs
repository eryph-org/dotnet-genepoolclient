using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GeneUploadResponse
{
    [JsonConstructor]
    public GeneUploadResponse(string Geneset, string Gene, GenePartUploadUri[] UploadUris)
    {
        this.Geneset = Geneset;
        this.Gene = Gene;
        this.UploadUris = UploadUris;
    }

    [JsonPropertyName("geneset")]
    public string Geneset { get; init; }

    [JsonPropertyName("gene")] public string Gene { get; init; }

    [JsonPropertyName("upload_uris")]
    public GenePartUploadUri[] UploadUris { get; init; }

    public void Deconstruct(out string geneset, out string gene, out GenePartUploadUri[] uploadUris)
    {
        geneset = Geneset;
        gene = Gene;
        uploadUris = UploadUris;
    }
}