namespace Eryph.GenePool.Model;

public static class KnownGenesetTagMetadataKeys
{
    public const string OsType = "_os_type";
    public const string OsName = "_os_name";

    public static string[] AllKeys => [OsType, OsName];
}