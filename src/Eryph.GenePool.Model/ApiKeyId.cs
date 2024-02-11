using System;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class ApiKeyId : NewType<ApiKeyId, string>
{
    private const int MaxLength = 36;
    private const int MinLength = 24;

    public ApiKeyId(string value) : base(Normalize(value))
    {
        _ = Validate(value).IfFail(err => throw new Exception(Error.Many(err).Message));
    }

    private ApiKeyId(string value, Unit _) : base(Normalize(value))
    {
    }

    public static ApiKeyId ParseUnsafe(string apiKey)
    {
        return TryParse(apiKey).Match(r => r,
            l =>
            {
                l.Throw();
                return default!;
            });
    }

    public static Either<Error, ApiKeyId> TryParse(string value) =>
        Validate(Normalize(value)).Map(_ => new ApiKeyId(value, Prelude.unit)).ToEither().MapLeft(Error.Many);

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static Validation<Error, string> Validate(string value) =>
        from length in Validations.ValidateLength(value, nameof(ApiKeyId), MaxLength, MinLength)
        select value;


}