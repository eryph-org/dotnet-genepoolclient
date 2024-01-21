using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record OrganizationRefResponse
{
    [JsonConstructor]
    public OrganizationRefResponse(string Name, Uri Uri)
    {
        this.Name = Name;
        this.Uri = Uri;
    }

    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("uri")] public Uri Uri { get; init; }

    public void Deconstruct(out string name, out Uri uri)
    {
        name = Name;
        uri = Uri;
    }
}