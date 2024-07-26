using System;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Client;

public record UpdateOrganizationBody
{
    [JsonPropertyName("name")]

    public string? Name { get; set; }

    [JsonPropertyName("owner_org_id")]

    public Guid? OrgId { get; set; }

    public string? ETag { get; set; }

}