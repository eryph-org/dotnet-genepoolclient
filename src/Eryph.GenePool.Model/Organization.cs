using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class Organization : NewType<Organization, string>
{
    private const int MaxLength = 40;
    private const int MinLength = 3;

    public Organization(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private Organization(string value, Unit _) : base(Normalize(value))
    {
    }

    public static Organization ParseUnsafe(string organization)
    {
        return TryParse(organization).Match(r => r,
            l =>
            {
                l.Throw();
                return default!;
            });
    }

    public static Either<Error, Organization> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new Organization(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(Organization), MaxLength, MinLength)
        from characters in Validations.ValidateCharacters(value, nameof(Organization))
        select value;


}