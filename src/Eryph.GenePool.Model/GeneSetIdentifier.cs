using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public record GeneSetIdentifier(Organization Organization, Geneset Geneset, Tag Tag)
{

    public string Name => $"{Organization}/{Geneset}/{Tag}";
    public string UntaggedName => $"{Organization}/{Geneset}";

    public static GeneSetIdentifier ParseUnsafe(string genesetName)
    {
        return Parse(genesetName).Match(r => r,
            l =>
            {
                l.Throw();
                return default!;
            });
    }

    public static Either<Error, GeneSetIdentifier> Parse(string genesetName)
    {
        genesetName = genesetName.ToLowerInvariant();
        var imageParts = genesetName.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (imageParts.Length != 3 && imageParts.Length != 2)
            return Error.New($"Invalid geneset name '{genesetName}'");

        var orgString = imageParts[0].ToLowerInvariant();
        var idString = imageParts[1].ToLowerInvariant();
        var tagString = imageParts.Length == 3 ? imageParts[2].ToLowerInvariant() : "latest";

        return from org in Organization.TryParse(orgString)
            from id in Geneset.TryParse(idString)
            from tag in Tag.TryParse(tagString)
            select new GeneSetIdentifier(org, id, tag);
    }

    public override string ToString()
    {
        return Name;
    }
}