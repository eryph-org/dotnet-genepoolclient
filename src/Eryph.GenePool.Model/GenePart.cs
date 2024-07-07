using Eryph.ConfigModel;

namespace Eryph.GenePool.Model;

public class GenePart : EryphName<GenePart>
{
    public GenePart(string value) : base(value)
    {
        ValidOrThrow(Validations<GenePart>.ValidateCharacters(
                         value,
                         allowHyphens: false,
                         allowUnderscores: false,
                         allowDots: false,
                         allowSpaces: false)
                     | Validations<GenePart>.ValidateLength(value, 40, 40));
    }
}
