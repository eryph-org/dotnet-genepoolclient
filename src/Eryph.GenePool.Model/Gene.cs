using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class Gene : NewType<Gene, string>
{
    private const int MaxLength = 64;
    private const int MinLength = 64;

    public Gene(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private Gene(string value, Unit _) : base(Normalize(value))
    {
    }

    public static Either<Error, Gene> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new Gene(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(Gene), MaxLength, MinLength)
        from characters in Validations.ValidateHash(value, nameof(Gene))
        select value;


}

