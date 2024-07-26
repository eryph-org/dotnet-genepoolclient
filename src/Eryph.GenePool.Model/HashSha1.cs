using Eryph.ConfigModel;

namespace Eryph.GenePool.Model;

public class HashSha1 : EryphName<HashSha1>
{
    public HashSha1(string value) : base(value)
    {
        ValidOrThrow(Validations<HashSha1>.ValidateCharacters(
                         value,
                         allowDots: false,
                         allowHyphens: false,
                         allowSpaces: false)
                     | Validations<GeneName>.ValidateLength(value, 40, 40));
    }
}