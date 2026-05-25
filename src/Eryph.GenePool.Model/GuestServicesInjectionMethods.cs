using System.Collections.Generic;

namespace Eryph.GenePool.Model;

/// <summary>
/// Strategies for adding eryph guest services to a substituted cloud image.
/// The set is intentionally open for future values.
/// </summary>
public static class GuestServicesInjectionMethods
{
    public const string Extension = "extension";
    public const string Geneset = "geneset";
    public const string None = "none";

    public static readonly IReadOnlyCollection<string> KnownNames =
        new[] { Extension, Geneset, None };
}
