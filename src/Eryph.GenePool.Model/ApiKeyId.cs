using System;
using Dbosoft.Functional.DataTypes;
using Eryph.ConfigModel;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;

namespace Eryph.GenePool.Model;

public class ApiKeyId : ValidatingNewType<ApiKeyId, string, OrdStringOrdinalIgnoreCase>
{
    public ApiKeyId(string value) : base(value)
    {
        ValidOrThrow(
            ConfigModel.Validations.ValidateLength(value, nameof(ApiKeyId), 20, 36)
            | Validations.ValidateKeyIdString(value, nameof(ApiKeyId)));
    }
}
