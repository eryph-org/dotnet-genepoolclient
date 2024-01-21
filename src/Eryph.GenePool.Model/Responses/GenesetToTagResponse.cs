using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record GenesetToTagResponse
{
    [JsonConstructor]
    public GenesetToTagResponse(string Name, string Tag)
    {
        this.Name = Name;
        this.Tag = Tag;
    }

    public string Name { get; init; }
    public string Tag { get; init; }

    public void Deconstruct(out string name, out string tag)
    {
        name = Name;
        tag = Tag;
    }
}