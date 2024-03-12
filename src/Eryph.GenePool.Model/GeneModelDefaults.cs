using System.Text.Json.Serialization;
using System.Text.Json;

namespace Eryph.GenePool.Model
{
    public class GeneModelDefaults
    {
        private static JsonSerializerOptions? _options;

        public static JsonSerializerOptions SerializerOptions
        {
            get
            {
                if (_options != null) return _options;

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                };
                _options = options;

                return _options;
            }
        }
    }
}
