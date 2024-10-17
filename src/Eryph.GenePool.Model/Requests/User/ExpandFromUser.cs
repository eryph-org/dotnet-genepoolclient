using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model.Requests.User;

public struct ExpandFromUser
{
    [JsonPropertyName("identity_orgs")]
    public bool IdentityOrgs { get; set; }

    [JsonPropertyName("genepool_orgs")]
    public bool GenepoolOrgs { get; set; }

}