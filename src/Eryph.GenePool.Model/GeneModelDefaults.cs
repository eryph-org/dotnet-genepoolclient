using System;
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

        public static Version LatestGeneManifestVersion = new (1, 1);
        public static Version LatestGenesetManifestVersion = new(1, 0);
        public static Version LatestGenesetTagManifestVersion = new(1, 1);

        public const int MaxYamlSourceBytes = 2 * 1024 * 1024;
        public const int MaxGenesetMarkdownBytes = 2 * 1024 * 1024;

        // consider max bytes used for storing geneset and gene metadata when
        // changing these values - technical max is (key + value) * count * 4 (utf-8)
        public const int MaxMetadataKeyLength = 40;
        public const int MaxMetadataKeyCount = 50;
        public const int MaxMetadataValueLength = 500;
        public const int MinCompressionBytes = 2 * 1024;

        public const int MaxCategoryCount = 3;
        public static int MaxTagCount = 3;
    }
}
