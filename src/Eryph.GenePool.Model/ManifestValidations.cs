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
               | ValidateProperty(manifest, m=>m.CatletGene, ValidateGeneHash, path)
               | ValidateList(manifest, m=>m.VolumeGenes, ValidateGeneReference, path, 0, 100)
               | ValidateList(manifest, m=>m.FodderGenes, ValidateGeneReference, path,0, 100)
               | ValidateProperty(manifest, m=> m.Metadata, Validations.ValidateMetadata, path);
    }

    public static Validation<ValidationIssue, Unit> ValidateGenesetManifest(GenesetManifestData manifest, string path = "")
    {
        return ValidateProperty(manifest, m => m.Version, Validations.ValidateVersionString, path)
               | ValidateProperty(manifest, m => m.Geneset, GeneSetIdentifier.NewValidation, path, true)
               | ValidateProperty(manifest, m => m.ShortDescription, Validations.ValidateGenesetShortDescription, path, true)
               | ValidateProperty(manifest, m => m.Description, Validations.ValidateGenesetDescription, path, true)
               | ValidateProperty(manifest, m => m.DescriptionMarkdown, Validations.ValidateMarkdownContentSize, path, true)
               | ValidateProperty(manifest, m => m.Metadata, Validations.ValidateMetadata, path);
    }

    public static Validation<ValidationIssue, Unit> ValidateGeneReference(GeneReferenceData reference, string path)
    {
        return
            ValidateProperty(reference, x => x.Name, GeneName.NewValidation, path, required: true)
            | ValidateProperty(reference, x => x.Hash, ValidateGeneHash, path, required: true);

    }

    public static Validation<Error, Unit> ValidateGeneHash(string hash)
    {
        return
            from gNull in guard(notEmpty(hash), Validations.BadRequestError("Gene hash is empty")).ToValidation()
            let splitHash = hash.Split(':')
            from gSep in guard(splitHash.Length == 2,
                Validations.BadRequestError("Gene hash has to contain one : as hash type separator")).ToValidation()
            from gType in guard(splitHash[0] is "sha1" or "sha256",
                Validations.BadRequestError("hash type has to be sha1 or sha256")).ToValidation()
            from hashLength in
                splitHash[0] == "sha1"
                    ? HashSha1.NewValidation(splitHash[1]).Map(_ => Unit.Default)
                    : HashSha256.NewValidation(splitHash[1]).Map(_ => Unit.Default)

            select Unit.Default;
    }



}