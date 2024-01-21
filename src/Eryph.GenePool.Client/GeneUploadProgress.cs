namespace Eryph.GenePool.Client;

public class GeneUploadProgress
{
    public string Part { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long PartSize { get; set; }
    public long TotalMissingSize { get; set; }
    public int TotalMissingCount { get; set; }
    public int TotalUploadedCount { get; set; }

    public bool Uploaded { get; set; }
    public long TotalUploadedSize { get; set; }

}