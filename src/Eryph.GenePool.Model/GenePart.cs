using System;
using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class GenePart : EryphName<GenePart>
{
    public GenePart(string value) : base(value)
    {
        ValidOrThrow(Validations<GenePart>.ValidateCharacters(
                         value,
                         allowHyphens: false,
                         allowDots: false,
                         allowSpaces: false)
                     | Validations<GenePart>.ValidateLength(value, 40, 40));
    }
}
