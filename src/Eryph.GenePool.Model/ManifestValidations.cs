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
               | ValidateProperty(manifest, m => m.Metadata, Validations.ValidateGenesetMetadata, path);
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
            from gType in guard(splitHash[0] is "sha1" or "sha256",
                Validations.BadRequestError("hash type has to be sha1 or sha256")).ToValidation()
            from hashValid in
                splitHash[0] == "sha1"
                    ? HashSha1.NewValidation(splitHash[1]).Map(_ => Unit.Default)
                    : HashSha256.NewValidation(splitHash[1]).Map(_ => Unit.Default)


            select Unit.Default;
    }


    public static Validation<Error, Unit> ValidateArchitecture(string architecture)
    {
        return
            from gNull in guard(notEmpty(architecture), Validations.BadRequestError("Architecture is empty")).ToValidation()
            from gVal in guard(string.Equals(architecture,Architectures.HyperVAmd64) 
                               || string.Equals(architecture, Architectures.HyperVAny) 
                               || string.Equals(architecture, Architectures.Any),
                Validations.BadRequestError($"Architecture value has to be '{Architectures.HyperVAmd64}'" +
                                            $", '{Architectures.HyperVAny}' " +
                                            $"or '{Architectures.Any}'")).ToValidation()

            select Unit.Default;
    }
}