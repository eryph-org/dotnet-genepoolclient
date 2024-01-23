using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public record GeneIdentifier(GeneType GeneType, GeneSetIdentifier GeneSet, string Gene)
{
    public string Name => $"{GeneSet.Name}:{Gene}";

    public static Either<Error, GeneIdentifier> Parse(GeneType geneType, string geneName)
    {
        geneName = geneName.ToLowerInvariant();
        var geneNameParts = geneName.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (geneNameParts.Length != 3 && geneNameParts.Length != 2)
            return Error.New($"Invalid gene name '{geneName}'");

        if (geneNameParts[0] == "gene")
            geneNameParts = geneNameParts.Skip(1).ToArray();

        return GeneSetIdentifier.Parse(geneNameParts[0])
            .Map(geneset => new GeneIdentifier(geneType, geneset, geneNameParts[1].ToLowerInvariant()));

    }

    public static GeneIdentifier ParseUnsafe(GeneType geneType, string geneName)
    {
        return Parse(geneType, geneName).Match(r => r,
            l =>
            {
                l.Throw();
                return default!;
            });
    }

    public override string ToString()
    {
        return $"{GeneType} {Name}";
    }
}