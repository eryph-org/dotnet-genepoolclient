using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class Geneset : NewType<Geneset, string>
{
    private const int MaxLength = 40;
    private const int MinLength = 3;

    public Geneset(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private Geneset(string value, Unit _) : base(Normalize(value))
    {
    }

    public static Geneset ParseUnsafe(string geneset)
    {
        return TryParse(geneset).Match(r => r,
            l =>
            {
                l.Throw();
                return default!;
            });
    }
    public static Either<Error, Geneset> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new Geneset(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(Geneset), MaxLength, MinLength)
        from characters in Validations.ValidateCharacters(value, nameof(Geneset))
        select value;

}