using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class Tag : NewType<Tag, string>
{
    private const int MaxLength = 20;
    private const int MinLength = 3;

    public Tag(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private Tag(string value, Unit _) : base(Normalize(value))
    {
    }

    public static Either<Error, Tag> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new Tag(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(Tag), MaxLength, MinLength)
        from characters in Validations.ValidateCharacters(value, nameof(Tag))
        select value;


}