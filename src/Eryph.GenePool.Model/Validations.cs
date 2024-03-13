using System;
using System.Net;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public static class Validations
{
    public static Validation<Error, string> ValidateKeyIdString(string value, string fieldName)
    {
        value = value.Replace('-', '+').Replace('_', '/');
        if (value.Length % 4 != 0) value += new string ('=', 4 - value.Length % 4);

        return Prelude.Try(()=>
            {
                var guidBytes = Convert.FromBase64String(value);
                return new Guid(guidBytes);
            }).ToEither(_ => Error.New("invalid format"))
        ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
            $"{fieldName} does not meet the requirements. Value has to be a key id.");
    }

    private static Error StatusCodeToError(HttpStatusCode statusCode, string? message = null) => Error.New((int)statusCode, message ?? statusCode.ToString());
}
