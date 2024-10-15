namespace Eryph.GenePool.Model;

public static class KnownGenesetMetadataKeys
{
    public const string Categories = "_categories";
    public const string Tags = "_tags";
    public const string OsTypes = "_os_types";

    public static string[] AllKeys => [Categories, Tags, OsTypes];
}