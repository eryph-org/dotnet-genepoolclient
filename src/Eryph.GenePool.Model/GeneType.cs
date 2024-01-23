using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneType
{
    Catlet,
    Volume,
    Fodder
}