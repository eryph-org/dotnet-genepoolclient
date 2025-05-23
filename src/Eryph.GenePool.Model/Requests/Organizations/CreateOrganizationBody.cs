using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.Organizations;

public class CreateOrganizationBody
{
    [JsonPropertyName("name")]

    public string? Name { get; set; }

    [JsonPropertyName("id")]

    public Guid? Id { get; set; }

    [JsonPropertyName("owner_org_id")]
    public Guid? OrgId { get; set; }

    [JsonPropertyName("owner_org_name")]
    public string? NewOrgName { get; set; }

}