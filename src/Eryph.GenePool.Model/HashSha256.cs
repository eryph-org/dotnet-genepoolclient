using Eryph.ConfigModel;

namespace Eryph.GenePool.Model;

public class HashSha256 : EryphName<HashSha256>
{
    public HashSha256(string value) : base(value)
    {
        ValidOrThrow(Validations<HashSha256>.ValidateCharacters(
                         value,
                         allowDots: false,
                         allowHyphens: false,
                         allowUnderscores: false,
                         allowSpaces: false)
                     | Validations<HashSha256>.ValidateLength(value, 64, 64)
                     | GeneValidations.ValidateHashCharacters(value));
    }
}