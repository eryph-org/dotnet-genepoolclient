using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public static class GeneValidations
{
    public static Validation<Error, Unit> ValidateGeneType(string geneTypeString)
    {
        var enumNames = Enum.GetNames(typeof(GeneType)).Select(x => x.ToLowerInvariant()).ToArray();

        return
            from gNull in Prelude.guard(Prelude.notEmpty(geneTypeString), Validations.BadRequestError("Gene type is empty")).ToValidation()
            from gVal in Prelude.guard(enumNames.Contains(geneTypeString.ToLowerInvariant()),
                Validations.BadRequestError($"Gene type has to be one of {string.Join(", ", enumNames)}")).ToValidation()
            select Unit.Default;

    }

    public static Validation<Error, Unit> ValidateGeneFormat(string formatString)
    {
        return
            from gNull in Prelude.guard(Prelude.notEmpty(formatString), Validations.BadRequestError("Gene format is empty")).ToValidation()
            from gVal in Prelude.guard(
                string.Equals(formatString, "xz") 
                || string.Equals(formatString, "gz") ||
                string.Equals(formatString, "plain"),
                Validations.BadRequestError($"Gene format has to be one of plain,gz,xz")).ToValidation()
            select Unit.Default;
    }

    public static Validation<Error, Unit> ValidateGeneFileName(string filename)
    {
        return ConfigModel.Validations.ValidateFileName(filename, "filename")
            .Map(_ => Unit.Default);
    }
}