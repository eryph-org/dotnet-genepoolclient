using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Responses;

public record OrganizationResponse
{
    [JsonConstructor]
    public OrganizationResponse(Guid Id, string Name, Guid? OrgId)
    {
        this.Id = Id;
        this.Name = Name;
        this.OrgId = OrgId;
    }

    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("owner_org_id")]
    public Guid? OrgId { get; init; }

    public void Deconstruct(out Guid id, out string name, out Guid? orgId)
    {
        id = Id;
        name = Name;
        orgId = OrgId;
    }
}