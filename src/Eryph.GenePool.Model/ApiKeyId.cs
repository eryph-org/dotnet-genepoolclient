using Dbosoft.Functional.DataTypes;
using LanguageExt.ClassInstances;

namespace Eryph.GenePool.Model;

public class ApiKeyId : ValidatingNewType<ApiKeyId, string, OrdStringOrdinal>
{
    public ApiKeyId(string value) : base(value)
    {
        ValidOrThrow(
            ConfigModel.Validations.ValidateLength(value, nameof(ApiKeyId), 20, 36)
            | Validations.ValidateKeyIdString(value, nameof(ApiKeyId)));
    }
}
