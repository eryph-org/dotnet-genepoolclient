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
        Validations<Gene>.ValidateLength(value, 64, 64);
    }
}
