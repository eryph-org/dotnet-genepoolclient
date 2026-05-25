using System;
using System.Collections.Generic;
using System.Linq;
using Dbosoft.Functional.Validations;
using Eryph.ConfigModel;
using LanguageExt;
using LanguageExt.Common;
using static Dbosoft.Functional.Validations.ComplexValidations;
using static LanguageExt.Prelude;

namespace Eryph.GenePool.Model;


public class ManifestValidations
{

    public static Validation<ValidationIssue, Unit> ValidateGenesetTagManifest(GenesetTagManifestData manifest, string path = "")
    {
        return ValidateProperty(manifest, m=> m.Version, Validations.ValidateVersionString, path)
               | ValidateProperty(manifest, m=>m.Geneset, GeneSetIdentifier.NewValidation, path, true)
               | ValidateProperty(manifest, m => m.Reference, GeneSetIdentifier.NewValidation, path)
               | ValidateProperty(manifest, m => m.Parent, GeneSetIdentifier.NewValidation, path)
               | ValidateProperty(manifest, m=>m.CatletGene, ValidateGeneReferenceProperty, path)
               | ValidateList(manifest, m=>m.VolumeGenes, ValidateGeneReference, path, 0, 100)
               | ValidateList(manifest, m=>m.FodderGenes, ValidateGeneReference, path,0, 100)
               | ValidateProperty(manifest, m=> m.Metadata, Validations.ValidateGenesetTagMetadata, path);
    }

    public static Validation<ValidationIssue, Unit> ValidateGenesetManifest(GenesetManifestData manifest, string path = "")
    {
        return ValidateProperty(manifest, m => m.Version, Validations.ValidateVersionString, path)
               | ValidateProperty(manifest, m => m.Geneset, GeneSetIdentifier.NewValidation, path, true)
               | ValidateProperty(manifest, m => m.ShortDescription, Validations.ValidateGenesetShortDescription, path, true)
               | ValidateProperty(manifest, m => m.Description, Validations.ValidateGenesetDescription, path)
               | ValidateProperty(manifest, m => m.DescriptionMarkdown, Validations.ValidateMarkdownContentSize, path)
               | ValidateProperty(manifest, m => m.Metadata, Validations.ValidateGenesetMetadata, path)
               | ValidateCloudCompatibility(manifest.CloudCompatibility, AppendPath(path, "cloud_compatibility"));
    }

    public static Validation<ValidationIssue, Unit> ValidateGeneManifest(GeneManifestData manifest, string path = "")
    {
        return ValidateProperty(manifest, m => m.Version, Validations.ValidateVersionString, path)
               | ValidateProperty(manifest, m => m.Name, GeneName.NewValidation, path, true)
               | ValidateProperty(manifest, m => m.Type, GeneValidations.ValidateGeneType, path, true)
               | ValidateProperty(manifest, m => m.Format, GeneValidations.ValidateGeneFormat, path)
               | ValidateProperty(manifest, m => m.FileName, GeneValidations.ValidateGeneFileName, path)
               | ValidateList(manifest, m => m.Parts, ValidateGeneHashProperty, path, 1)
               | ValidateProperty(manifest, m => m.Architecture,ValidateArchitecture, path);
    }

    public static Validation<ValidationIssue, Unit> ValidateGeneReference(GeneReferenceData reference, string path)
    {
        return
            ValidateProperty(reference, x => x.Name, GeneName.NewValidation, path, required: true)
            | ValidateProperty(reference, x => x.Hash, ValidateReferenceHash, path, required: true)
            | ValidateProperty(reference, x => x.Architecture, ValidateArchitecture, path, required: true);


    }

    public static Validation<ValidationIssue, Unit> ValidateGeneHashProperty(string hash, string path)
    {
        return ValidateGeneHash(hash).MapFail(e => new ValidationIssue(path, e.Message));
    }

    public static Validation<ValidationIssue, Unit> ValidateGeneReferenceProperty(string hash, string path)
    {
        return ValidateReferenceHash(hash).MapFail(e => new ValidationIssue(path, e.Message));
    }

    public static Validation<Error, Unit> ValidateGeneHash(string hash)
    {
        return
            from gNull in guard(notEmpty(hash), Validations.BadRequestError("Gene hash is empty")).ToValidation()
            let splitHash = hash.Split(':')
            from gSep in guard(splitHash.Length == 2,
                Validations.BadRequestError("Gene hash has to contain one : as hash type separator")).ToValidation()
            from gType in guard(splitHash[0] is "sha1",
                Validations.BadRequestError("hash type has to be sha1")).ToValidation()
            from hashValid in HashSha1.NewValidation(splitHash[1]).Map(_ => Unit.Default)
            

            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateReferenceHash(string hash)
    {
        return
            from gNull in guard(notEmpty(hash), Validations.BadRequestError("Gene reference hash is empty")).ToValidation()
            let splitHash = hash.Split(':')
            from gSep in guard(splitHash.Length == 2,
                Validations.BadRequestError("Gene reference hash has to contain one : as hash type separator")).ToValidation()
            from gType in guard(splitHash[0] is "sha256",
                Validations.BadRequestError("hash type has to be sha256")).ToValidation()
            from hashValid in HashSha256.NewValidation(splitHash[1]).Map(_ => Unit.Default)


            select Unit.Default;
    }


    public static Validation<Error, Unit> ValidateArchitecture(string architecture)
    {
        return
            from gNull in guard(notEmpty(architecture), Validations.BadRequestError("Architecture is empty")).ToValidation()
            from gVal in guard(Architectures.KnownNames.Contains(architecture),
                Validations.BadRequestError(
                    $"Architecture value has to be one of {string.Join(", ", Architectures.KnownNames.Select(n => $"'{n}'"))}."))
                .ToValidation()

            select Unit.Default;
    }

    public static Validation<ValidationIssue, Unit> ValidateCloudCompatibility(
        Dictionary<string, CloudImageReference[]>? cloudCompatibility, string path)
    {
        if (cloudCompatibility is null)
            return Success<ValidationIssue, Unit>(Unit.Default);

        return cloudCompatibility.Aggregate(
            Success<ValidationIssue, Unit>(Unit.Default),
            (acc, kv) =>
            {
                var drivePath = AppendPath(path, kv.Key);
                var keyValidation = ValidateCloudCompatibilityDriveName(kv.Key)
                    .MapFail(e => new ValidationIssue(drivePath, e.Message));
                var entriesValidation = kv.Value is null
                    ? Fail<ValidationIssue, Unit>(new ValidationIssue(drivePath,
                        "Cloud compatibility entries must be an array."))
                    : kv.Value
                        .Select((entry, i) =>
                        {
                            var entryPath = $"{drivePath}[{i}]";
                            return entry is null
                                ? Fail<ValidationIssue, Unit>(new ValidationIssue(entryPath,
                                    "Cloud image reference must not be null."))
                                : ValidateCloudImageReference(entry, entryPath);
                        })
                        .Aggregate(Success<ValidationIssue, Unit>(Unit.Default), (a, b) => a | b);
                return acc | keyValidation | entriesValidation;
            });
    }

    public static Validation<ValidationIssue, Unit> ValidateCloudImageReference(
        CloudImageReference reference, string path)
    {
        return ValidateProperty(reference, x => x.Cloud, ValidateCloud, path, required: true)
               | ValidateProperty(reference, x => x.Type, ValidateCloudImageReferenceType, path, required: true)
               | ValidateProperty(reference, x => x.GuestServicesInjection, ValidateGuestServicesInjection, path)
               | ValidateCloudImageReferenceBody(reference, path);
    }

    private static Validation<ValidationIssue, Unit> ValidateCloudImageReferenceBody(
        CloudImageReference reference, string path)
    {
        Validation<ValidationIssue, Unit> Require(string? value, string field) =>
            guard(!string.IsNullOrWhiteSpace(value),
                    new ValidationIssue(path,
                        $"'{field}' is required for cloud image reference type '{reference.Type}'."))
                .ToValidation();

        Validation<ValidationIssue, Unit> RequireCloud(string expected) =>
            guard(string.Equals(reference.Cloud, expected, StringComparison.Ordinal),
                    new ValidationIssue(path,
                        $"Cloud image reference type '{reference.Type}' requires cloud '{expected}'."))
                .ToValidation();

        return reference.Type switch
        {
            CloudImageReferenceTypes.AzureMarketplace =>
                RequireCloud(Hypervisors.Azure)
                | Require(reference.Publisher, "publisher")
                | Require(reference.Offer, "offer")
                | Require(reference.Sku, "sku")
                | ValidateAzurePlan(reference.Plan, path),
            CloudImageReferenceTypes.Ec2SsmParameter =>
                RequireCloud(Hypervisors.EC2)
                | Require(reference.Parameter, "parameter"),
            CloudImageReferenceTypes.Ec2ImageFilter =>
                RequireCloud(Hypervisors.EC2)
                | Require(reference.Owner, "owner")
                | Require(reference.NamePattern, "name_pattern"),
            _ => Success<ValidationIssue, Unit>(Unit.Default),
        };
    }

    private static Validation<ValidationIssue, Unit> ValidateAzurePlan(AzurePlan? plan, string path)
    {
        if (plan is null)
            return Success<ValidationIssue, Unit>(Unit.Default);

        Validation<ValidationIssue, Unit> Require(string? value, string field) =>
            guard(!string.IsNullOrWhiteSpace(value),
                    new ValidationIssue(path, $"'plan.{field}' is required for an Azure marketplace plan."))
                .ToValidation();

        return Require(plan.Publisher, "publisher")
               | Require(plan.Product, "product")
               | Require(plan.Name, "name");
    }

    public static Validation<Error, Unit> ValidateCloudCompatibilityDriveName(string driveName)
    {
        return
            from gNull in guard(notEmpty(driveName),
                Validations.BadRequestError("Cloud compatibility drive name is empty")).ToValidation()
            from gLen in guard(driveName.Length <= 50,
                Validations.BadRequestError("Cloud compatibility drive name is too long (max. 50 chars).")).ToValidation()
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateCloud(string cloud)
    {
        return
            from gNull in guard(notEmpty(cloud), Validations.BadRequestError("Cloud is empty")).ToValidation()
            from gVal in guard(cloud is Hypervisors.Azure or Hypervisors.EC2,
                    Validations.BadRequestError(
                        $"Cloud value has to be one of '{Hypervisors.Azure}', '{Hypervisors.EC2}'."))
                .ToValidation()
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateCloudImageReferenceType(string type)
    {
        return
            from gNull in guard(notEmpty(type),
                Validations.BadRequestError("Cloud image reference type is empty")).ToValidation()
            from gVal in guard(CloudImageReferenceTypes.KnownNames.Contains(type),
                    Validations.BadRequestError(
                        $"Cloud image reference type has to be one of {string.Join(", ", CloudImageReferenceTypes.KnownNames.Select(n => $"'{n}'"))}."))
                .ToValidation()
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateGuestServicesInjection(string method)
    {
        return
            from gNull in guard(notEmpty(method),
                Validations.BadRequestError("Guest services injection method is empty")).ToValidation()
            from gVal in guard(GuestServicesInjectionMethods.KnownNames.Contains(method),
                    Validations.BadRequestError(
                        $"Guest services injection method has to be one of {string.Join(", ", GuestServicesInjectionMethods.KnownNames.Select(n => $"'{n}'"))}."))
                .ToValidation()
            select Unit.Default;
    }

    private static string AppendPath(string path, string segment) =>
        string.IsNullOrEmpty(path) ? segment : $"{path}.{segment}";
}
