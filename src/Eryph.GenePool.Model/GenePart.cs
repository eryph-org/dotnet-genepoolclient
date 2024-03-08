using System;
using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class GenePart : ValidatingNewType<GenePart, string, OrdStringOrdinalIgnoreCase>
{
    public GenePart(string value) : base(value)
    {
        ValidOrThrow(Validations<Gene>.ValidateCharacters(
                         value,
                         allowUpperCase: false,
                         allowHyphens: false,
                         allowDots: false,
                         allowSpaces: false)
                     | Validations<Gene>.ValidateLength(value, 40, 40));
    }
}
