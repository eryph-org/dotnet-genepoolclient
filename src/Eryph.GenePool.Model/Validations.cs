using System;
using System.Net;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public static class Validations
{
    public static readonly Regex NameRegex = new("^[a-z0-9]+(-[a-z0-9]+)*(.[a-z0-9]+)*$", RegexOptions.Compiled);
    public static readonly Regex HashRegex = new("^[a-z0-9]*$", RegexOptions.Compiled);

    public static Validation<Error, string> ValidateLength(string value, string fieldName, int maxLength, int minLength) =>
        from maxLengthOut in ValidateMaxLength(value, fieldName, maxLength)
        from minLengthOut in ValidateMinLength(value, fieldName, minLength)
        select value;

    public static Validation<Error, string> ValidateMinLength(string value, string fieldName, int minLength) =>
        value.Length >= minLength
            ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
                      $"{fieldName} is shorter that minimum length of {minLength} characters.");

    public static Validation<Error, string> ValidateMaxLength(string value, string fieldName, int maxLength) =>
        value.Length <= maxLength
            ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
                $"{fieldName} is longer that max length of {maxLength} characters.");

    public static Validation<Error, string> ValidateCharacters(string value, string fieldName) =>
        NameRegex.IsMatch(value)
            ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
                $"{fieldName} does not meet the requirements. Only alphanumeric characters, numbers, dots and non-consecutive hyphens are permitted. Hyphens and dots must not appear at the beginning or end.");


    public static Validation<Error, string> ValidateHash(string value, string fieldName) =>
        NameRegex.IsMatch(value)
            ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
                $"{fieldName} does not meet the requirements. Only alphanumeric characters and numbers are permitted.");

    public static Validation<Error, string> ValidateIsGuid(string value, string fieldName) =>
        Guid.TryParse(value, out _)
            ? Prelude.Success<Error, string>(value)
            : StatusCodeToError(HttpStatusCode.BadRequest,
                $"{fieldName} does not meet the requirements. Value has to be a guid.");


    private static Error StatusCodeToError(HttpStatusCode statusCode, string? message = null) => Error.New((int)statusCode, message ?? statusCode.ToString());

}