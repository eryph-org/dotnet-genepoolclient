using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class GenePart : NewType<GenePart, string>
{
    private const int MaxLength = 40;
    private const int MinLength = 40;

    public GenePart(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private GenePart(string value, Unit _) : base(Normalize(value))
    {
    }

    public static Either<Error, GenePart> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new GenePart(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(GenePart), MaxLength, MinLength)
        from characters in Validations.ValidateHash(value, nameof(GenePart))
        select value;


}