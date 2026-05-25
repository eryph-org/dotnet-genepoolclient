using System.Collections.Generic;

namespace Eryph.GenePool.Model;

public static class CloudImageReferenceTypes
{
    public const string AzureMarketplace = "azure_marketplace";
    public const string Ec2SsmParameter = "ec2_ssm_parameter";
    public const string Ec2ImageFilter = "ec2_image_filter";

    public static readonly IReadOnlyCollection<string> KnownNames =
        new[] { AzureMarketplace, Ec2SsmParameter, Ec2ImageFilter };
}
