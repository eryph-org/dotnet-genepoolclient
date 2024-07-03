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
                    ? guard(splitHash[1].Length == 40, Validations.BadRequestError("sha1 hash has to be 40 characters long"))
                        .ToValidation()
                    : guard(splitHash[1].Length == 64, Validations.BadRequestError("sha256 hash has to be 64 characters long"))
                        .ToValidation()

            select Unit.Default;
    }
}