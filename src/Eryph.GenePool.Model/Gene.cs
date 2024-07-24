using System;
using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class Gene : EryphName<Gene>
{
    public Gene(string value) : base(value)
    {
        ValidOrThrow(Validations<Gene>.ValidateCharacters(
                        value,
                        allowHyphens: false,
                        allowUnderscores: false,
                        allowDots: false,
                        allowSpaces: false)
                    | Validations<Gene>.ValidateLength(value, 64, 64));
    }
}
