using System;
using System.Collections.Generic;
using System.Linq;
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

    public static Validation<Error, Unit> ValidateGenesetMetadata(IDictionary<string, string>? metadata)
    {
        return ValidateMetadata(true, metadata);
    }

    public static Validation<Error, Unit> ValidateGenesetTagMetadata(IDictionary<string, string>? metadata)
    {
        return ValidateMetadata(false, metadata);
    }

    public static Validation<Error, Unit> ValidateMetadata(bool geneset, IDictionary<string, string>? metadata)
    {
        if (metadata == null)
            return Unit.Default;

        if (metadata.Keys.Count > GeneModelDefaults.MaxMetadataKeyCount)
            return BadRequestError($"Metadata is too large. Only up to {GeneModelDefaults.MaxMetadataKeyCount} keys are allowed.");

        foreach (var kv in metadata)
        {
            if (kv.Key.Length > GeneModelDefaults.MaxMetadataKeyLength)
                return BadRequestError(
                    $"Metadata key '{kv.Key[..10]}..' is too long. Max length of keys is {GeneModelDefaults.MaxMetadataKeyLength}.");

            if (kv.Value.Length > GeneModelDefaults.MaxMetadataValueLength)
                return BadRequestError(
                    $"Metadata value for key '{kv.Key}' is too long. Max length of values is {GeneModelDefaults.MaxMetadataValueLength}.");

            // Check for reserved keys
            if (kv.Key.StartsWith("_"))
            {
                if (geneset)
                {
                    if (!KnownGenesetMetadataKeys.AllKeys.Contains(kv.Key))
                        return BadRequestError(
                            $"Metadata key '{kv.Key}' is not allowed: key names with underscore are reserved.");

                    switch (kv.Key)
                    {
                        case KnownGenesetMetadataKeys.Categories:
                        {
                            var names = kv.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (names.Length > GeneModelDefaults.MaxCategoryCount)
                                return BadRequestError(
                                    $"Too many categories. Only up to {GeneModelDefaults.MaxCategoryCount} categories are allowed.");
                            foreach (var name in names)
                            {
                                if (!CategoryNames.AllKeys.Contains(name))
                                    return BadRequestError($"Category '{name}' is not allowed.");
                            }

                            break;
                        }
                        case KnownGenesetMetadataKeys.OsTypes:
                        {
                            var names = kv.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var name in names)
                            {
                                if (!OsTypeNames.AllKeys.Contains(name))
                                    return BadRequestError($"OS type '{name}' is not allowed.");
                            }

                            break;
                        }
                        case KnownGenesetMetadataKeys.Tags:
                        {
                            var names = kv.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (names.Length > GeneModelDefaults.MaxTagCount)
                                return BadRequestError(
                                    $"Too many tags. Only up to {GeneModelDefaults.MaxTagCount} tags are allowed.");
                            break;
                        }
                    }
                }
                else
                {
                    if (!KnownGenesetTagMetadataKeys.AllKeys.Contains(kv.Key))
                        return BadRequestError(
                            $"Metadata key '{kv.Key}' is not allowed: key names with underscore are reserved.");

                    switch (kv.Key)
                    {

                        case KnownGenesetTagMetadataKeys.OsType:
                        {
                            if (!OsTypeNames.AllKeys.Contains(kv.Value))
                                return BadRequestError($"OS type '{kv.Value}' is not allowed.");

                            break;
                        }
                    }

                }
            }
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
                "Short description is too long (max. 90 chars allowed).");
        return Some(description);
    }

    public static Validation<Error, Unit> ValidateVersionString(string? version)
        => guard(version is not null && 
                 notEmpty(version) && 
                 Version.TryParse(version, out _),
            BadRequestError($"'{version}' is not a valid version")).ToValidation();

    private static Error StatusCodeToError(HttpStatusCode statusCode, string? message = null) =>
        Error.New((int)statusCode, message ?? statusCode.ToString());

    public static Error BadRequestError(string? message = null) =>
        StatusCodeToError(HttpStatusCode.BadRequest, message);

}