using System;
using System.Collections.Generic;
using System.Net;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Eryph.GenePool.Model;

public static class Validations
{
    public static Validation<Error, string> ValidateKeyIdString(string value, string fieldName)
    {
        value = value.Replace('-', '+').Replace('_', '/');
        if (value.Length % 4 != 0) value += new string('=', 4 - value.Length % 4);

        return Try(() =>
        {
            var guidBytes = Convert.FromBase64String(value);
            return new Guid(guidBytes);
        }).ToEither(_ => Error.New("invalid format"))
            ? Success<Error, string>(value)
            : BadRequestError($"{fieldName} does not meet the requirements. Value has to be a key id.");
    }

    public static Validation<Error, Unit> ValidateMarkdownContentSize(string? content)
    {
        if (content == null)
            return Unit.Default;

        var bytes = System.Text.Encoding.UTF8.GetByteCount(content);
        if (bytes > GeneModelDefaults.MaxGenesetMarkdownBytes)
            return BadRequestError(
                $"Markdown description is too large. Max size is {GeneModelDefaults.MaxGenesetMarkdownBytes} bytes.");

        return Unit.Default;
    }

    public static Validation<Error, Unit> ValidateYamlContentSize(string? content)
    {
        if (content == null)
            return Unit.Default;

        var bytes = System.Text.Encoding.UTF8.GetByteCount(content);
        if (bytes > GeneModelDefaults.MaxYamlSourceBytes)
            return BadRequestError(
                $"YAML content is too large. Max size is {GeneModelDefaults.MaxYamlSourceBytes} bytes.");

        return Unit.Default;
    }

    public static Validation<Error, Unit> ValidateMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata == null)
            return Unit.Default;

        if (metadata.Keys.Count > 10)
            return BadRequestError("Metadata is too large. Only up to 10 keys are allowed.");

        foreach (var kv in metadata)
        {
            if (kv.Key.Length > GeneModelDefaults.MaxMetadataKeyLength)
                return BadRequestError(
                    $"Metadata key '{kv.Key}' is too long. Max length of keys is {GeneModelDefaults.MaxMetadataKeyLength}.");

            if (kv.Value.Length > GeneModelDefaults.MaxMetadataValueLength)
                return BadRequestError(
                    $"Metadata value for key '{kv.Key}' is too long. Max length of values is {GeneModelDefaults.MaxMetadataValueLength}.");
        }

        return Unit.Default;
    }

    public static Validation<Error, Option<string>> ValidateGenesetDescription(string? description)
    {
        if (description == null)
            return Option<string>.None;

        if (description.Length > 200)
            return BadRequestError(
                "Description is too long (max. 200 chars). Use markdown description for longer content.");
        return Some(description);
    }

    public static Validation<Error, Option<string>> ValidateGenesetShortDescription(string? description)
    {
        if (description == null)
            return Option<string>.None;

        if (description.Length > 90)
            return BadRequestError(
                $"Short description is too long (max. 90 chars allowed).");
        return Some(description);
    }

    public static Validation<Error, Unit> ValidateVersionString(string? version)
        => guard(notEmpty(version) &&
                 Version.TryParse(version ?? "", out _),
            BadRequestError($"'{version}' is not a valid version")).ToValidation();

    private static Error StatusCodeToError(HttpStatusCode statusCode, string? message = null) =>
        Error.New((int)statusCode, message ?? statusCode.ToString());

    public static Error BadRequestError(string? message = null) =>
        StatusCodeToError(HttpStatusCode.BadRequest, message);

}