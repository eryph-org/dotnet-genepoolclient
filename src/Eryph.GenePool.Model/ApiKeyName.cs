using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt.ClassInstances;

namespace Eryph.GenePool.Model;

public class ApiKeyName : ValidatingNewType<ApiKeyName, string, OrdStringOrdinalIgnoreCase>
{
    public ApiKeyName(string value) : base(value)
    {
        ValidOrThrow(
            // until ValidateNoControlChars has been moved to Validations<T>
            Validations.ValidateNoControlChars(value, "APIKey Name")
                .Map(option => option.IfNone(""))
            | Validations<ApiKeyName>.ValidateLength(value, 1, 50));
    }

}
