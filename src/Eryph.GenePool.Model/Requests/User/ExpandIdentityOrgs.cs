using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.User;

public struct ExpandIdentityOrgs
{
    [JsonPropertyName("name")]
    public bool Name { get; set; }

    [JsonPropertyName("customer_no")]
    public bool CustomerNo { get; set; }

}