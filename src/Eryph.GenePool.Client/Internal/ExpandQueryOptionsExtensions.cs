using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Client.Internal
{
    internal static class ExpandQueryOptionsExtensions
    {
        public static RawRequestUriBuilder AddExpandOptions<T>(
            this RawRequestUriBuilder uri,T expand, string queryName = "expand" )
        where T : struct
        {
            var expandOptions = new StringBuilder();
            var expandType = typeof(T);
            var properties = expandType.GetProperties()
                .Where(p => p.PropertyType == typeof(bool));

            foreach (var property in properties)
            {
                var value = property.GetValue(expand);
                if (value is not true) continue;

                if (expandOptions.Length > 0)
                    expandOptions.Append(",");

                var name = property.Name;

                if (property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                        .FirstOrDefault() is JsonPropertyNameAttribute jsonNameAttribute)
                    name = jsonNameAttribute.Name;

                expandOptions.Append(name);
            }

            if (expandOptions.Length > 0)
                uri.AppendQuery(queryName, expandOptions.ToString());

            return uri;
        }
    }
}
