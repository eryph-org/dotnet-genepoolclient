using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt.ClassInstances;

namespace Eryph.GenePool.Model;

public class ApiKeyName : ValidatingNewType<ApiKeyName, string, OrdStringOrdinalIgnoreCase>
{
    public ApiKeyName(string value) : base(Normalize(value)!)
    {
        ValidOrThrow(Validations<ApiKeyName>.ValidateCharacters(
                         value,
                         allowHyphens: true,
                         allowUnderscores: true,
                         allowDots: false,
                         allowSpaces: false)
                     | Validations<ApiKeyName>.ValidateLength(value, 1, 50));
    }

    private static string? Normalize(string value) => value?.ToLowerInvariant();
}
