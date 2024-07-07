using Eryph.ConfigModel;

namespace Eryph.GenePool.Model;

public class Gene : EryphName<Gene>
{
    public Gene(string value) : base(value)
    {
        ValidOrThrow(Validations<Gene>.ValidateCharacters(
                         value,
                         allowHyphens: false,
                         allowDots: false,
                         allowUnderscores: false,
                         allowSpaces: false)
                     | Validations<Gene>.ValidateLength(value, 64, 64));
    }
}
