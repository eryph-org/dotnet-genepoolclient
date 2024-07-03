using System;
using System.Collections.Generic;
using System.Net;
using Eryph.ConfigModel;
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

    private static Error StatusCodeToError(HttpStatusCode statusCode, string? message = null) =>
        Error.New((int)statusCode, message ?? statusCode.ToString());

    private static Error BadRequestError(string? message = null) =>
        StatusCodeToError(HttpStatusCode.BadRequest, message);

    public static Validation<Error, Unit> ValidateGenesetTagManifest(GenesetTagManifestData manifest)
    {
        return
            from vVersion in guard( isEmpty(manifest.Version) ||
                    Version.TryParse(manifest.Version ?? "", out _),
                               BadRequestError($"Version {manifest.Version} is not supported."))
            from vGeneset in GeneSetIdentifier.NewValidation(manifest.Geneset)
            from vParent in notEmpty(manifest.Parent)
                ? GeneSetIdentifier.NewValidation(manifest.Geneset).Map(_ => Unit.Default)
                : Unit.Default

            from vReference in notEmpty(manifest.Reference)
                ? GeneSetIdentifier.NewValidation(manifest.Reference).Map(_ => Unit.Default)
                : Unit.Default

            from vCatlet in notEmpty(manifest.CatletGene) ? ValidateGeneHash(manifest.CatletGene) : Unit.Default

            from vVolumes in notDefault(manifest.VolumeGenes)
                ? ValidateGeneReferences(manifest.VolumeGenes)
                : Unit.Default

            from vFodder in notDefault(manifest.FodderGenes)
                ? ValidateGeneReferences(manifest.FodderGenes)
                : Unit.Default

            from vMetadata in ValidateMetadata(manifest.Metadata)
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateGeneReferences(GeneReferenceData[] references)
    {
        return references.Map(ValidateGeneReference)
            .Traverse(l => l).Map(_ => Unit.Default);
    }

    public static Validation<Error, Unit> ValidateGeneReference(GeneReferenceData reference)
    {
        return
            from uName in GeneName.NewValidation(reference.Name)
            from gHash in ValidateGeneHash(reference.Hash)
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateGeneHash(string hash)
    {
        return
            from gNull in guardnot(notEmpty(hash), BadRequestError("Gene hash is empty")).ToValidation()
            let splitHash = hash.Split(':')
            from gSep in guard(splitHash.Length == 2,
                BadRequestError("Gene hash has to contain : as hash type separator")).ToValidation()
            from gType in guard(splitHash[0] == "sha1" || splitHash[0] == "sha256",
                BadRequestError("hash type has to be sha1 or sha256")).ToValidation()
            from hashLength in
                splitHash[0] == "sha1"
                    ? guard(splitHash[1].Length == 40, BadRequestError("sha1 hash has to be 40 characters long"))
                        .ToValidation()
                    : guard(splitHash[1].Length == 64, BadRequestError("sha256 hash has to be 64 characters long"))
                        .ToValidation()

            select Unit.Default;
    }
}
