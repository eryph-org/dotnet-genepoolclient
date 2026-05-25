using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;

/// <summary>
/// A single candidate cloud image that may be substituted for a base catlet's
/// primary volume when building on a public cloud. Each reference is fully
/// self-contained and identified by the <see cref="Cloud"/> + <see cref="Type"/>
/// discriminators which together select the meaningful body fields.
/// </summary>
public class CloudImageReference
{
    /// <summary>
    /// The target cloud, one of <see cref="Hypervisors.Azure"/> or
    /// <see cref="Hypervisors.EC2"/>.
    /// </summary>
    [JsonPropertyName("cloud")]
    public string? Cloud { get; set; }

    /// <summary>
    /// The reference shape, one of <see cref="CloudImageReferenceTypes"/>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Strategy for injecting eryph guest services into the substituted image,
    /// one of <see cref="GuestServicesInjectionMethods"/>.
    /// </summary>
    [JsonPropertyName("guest_services_injection")]
    public string? GuestServicesInjection { get; set; }

    // azure-marketplace

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("offer")]
    public string? Offer { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("plan")]
    public AzurePlan? Plan { get; set; }

    // ec2-ssm-parameter

    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    // ec2-image-filter

    [JsonPropertyName("owner")]
    public string? Owner { get; set; }

    [JsonPropertyName("name_pattern")]
    public string? NamePattern { get; set; }

    [JsonPropertyName("architecture")]
    public string? Architecture { get; set; }
}
