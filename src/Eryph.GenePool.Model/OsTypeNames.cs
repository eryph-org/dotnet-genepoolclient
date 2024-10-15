namespace Eryph.GenePool.Model;

public static class OsTypeNames
{
    public const string Windows = "windows";
    public const string Linux = "linux";
    public const string FreeBSD = "freebsd";

    public static string[] AllKeys => [Windows, Linux, FreeBSD];

}