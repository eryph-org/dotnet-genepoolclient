using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Client.Internal;

internal static class ExpandQueryOptionsExtensions
{
    public static RawRequestUriBuilder AddExpandOptions<T>(
        this RawRequestUriBuilder uri,T expand, string queryName = "expand" )
        where T : struct
    {
        var expandOptions = new StringBuilder();

        ExpandProperty(expand, typeof(T), expandOptions, "");
        if (expandOptions.Length > 0)
            uri.AppendQuery(queryName, expandOptions.ToString());

        return uri;
    }

    // ReSharper disable once UnusedParameter.Local
    private static bool ExpandProperty(object? expand, Type expandType, StringBuilder expandOptions, string parentName)
    {
        var properties = expandType.GetProperties();
        var expanded = false;
        foreach (var property in properties)
        {
            var value = property.GetValue(expand);

            var innerType = property.PropertyType;
            if (innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                innerType = innerType.GetGenericArguments()[0];
            }

            var name = property.Name;

            if (property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                    .FirstOrDefault() is JsonPropertyNameAttribute jsonNameAttribute)
                name = jsonNameAttribute.Name;

            var optionOn = false;
            if (innerType != typeof(bool))
            {
                if (value != null)
                {
                    if (ExpandProperty(value, innerType, expandOptions, parentName + name + "."))
                    {
                        expanded = true;
                        continue; // already expanded sub properties, so skip this one
                    }

                    optionOn = true;
                }
            }
            else
                optionOn = (bool)value;
                
            if (optionOn is not true) continue;

            if (expandOptions.Length > 0)
                expandOptions.Append(",");

            expandOptions.Append(parentName);
            expandOptions.Append(name);
            expanded = true;
        }

        return expanded;
    }
}