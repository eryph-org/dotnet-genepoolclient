using System.Text.Json.Serialization;

namespace Eryph.GenePool.Packing;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneType
{
    Catlet,
    Volume,
    Fodder
}