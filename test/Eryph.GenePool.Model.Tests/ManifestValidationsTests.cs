using System.Text.Json;
using FluentAssertions;
using FluentAssertions.LanguageExt;

namespace Eryph.GenePool.Model.Tests;

public class ManifestValidationsTests
{
    [Theory]
    [InlineData(Architectures.HyperVAmd64)]
    [InlineData(Architectures.HyperVAny)]
    [InlineData(Architectures.KvmAmd64)]
    [InlineData(Architectures.KvmAny)]
    [InlineData(Architectures.AzureAmd64)]
    [InlineData(Architectures.AzureAny)]
    [InlineData(Architectures.EC2Amd64)]
    [InlineData(Architectures.EC2Any)]
    [InlineData(Architectures.Any)]
    public void ValidateArchitecture_KnownValue_ReturnsSuccess(string value)
    {
        var result = ManifestValidations.ValidateArchitecture(value);

        result.Should().BeSuccess();
    }

    [Theory]
    [InlineData("")]
    [InlineData("kvm")]
    [InlineData("hyperv/arm64")]
    [InlineData("xen/amd64")]
    public void ValidateArchitecture_UnknownValue_ReturnsFail(string value)
    {
        var result = ManifestValidations.ValidateArchitecture(value);

        result.Should().BeFail();
    }

    private static CloudImageReference AzureMarketplaceReference() => new()
    {
        Cloud = Hypervisors.Azure,
        Type = CloudImageReferenceTypes.AzureMarketplace,
        Publisher = "MicrosoftWindowsServer",
        Offer = "WindowsServer",
        Sku = "2022-datacenter-g2",
        Version = "latest",
        GuestServicesInjection = GuestServicesInjectionMethods.Extension,
    };

    private static CloudImageReference Ec2SsmReference() => new()
    {
        Cloud = Hypervisors.EC2,
        Type = CloudImageReferenceTypes.Ec2SsmParameter,
        Parameter = "/aws/service/ami-windows-latest/Windows_Server-2022-English-Full-Base",
        GuestServicesInjection = GuestServicesInjectionMethods.Geneset,
    };

    private static CloudImageReference Ec2FilterReference() => new()
    {
        Cloud = Hypervisors.EC2,
        Type = CloudImageReferenceTypes.Ec2ImageFilter,
        Owner = "amazon",
        NamePattern = "Windows_Server-2022-English-Full-Base-*",
        GuestServicesInjection = GuestServicesInjectionMethods.Geneset,
    };

    [Fact]
    public void ValidateCloudImageReference_ValidAzureMarketplace_ReturnsSuccess()
    {
        var result = ManifestValidations.ValidateCloudImageReference(AzureMarketplaceReference(), "");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudImageReference_ValidEc2SsmParameter_ReturnsSuccess()
    {
        var result = ManifestValidations.ValidateCloudImageReference(Ec2SsmReference(), "");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudImageReference_ValidEc2ImageFilter_ReturnsSuccess()
    {
        var result = ManifestValidations.ValidateCloudImageReference(Ec2FilterReference(), "");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudImageReference_MissingRequiredField_ReturnsFail()
    {
        var reference = AzureMarketplaceReference();
        reference.Sku = null;

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudImageReference_UnknownCloud_ReturnsFail()
    {
        var reference = AzureMarketplaceReference();
        reference.Cloud = "gcp";

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudImageReference_UnknownType_ReturnsFail()
    {
        var reference = AzureMarketplaceReference();
        reference.Type = "azure-shared-gallery";

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudImageReference_CloudTypeMismatch_ReturnsFail()
    {
        var reference = Ec2SsmReference();
        reference.Cloud = Hypervisors.Azure;

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Theory]
    [InlineData(GuestServicesInjectionMethods.Extension)]
    [InlineData(GuestServicesInjectionMethods.Geneset)]
    [InlineData(GuestServicesInjectionMethods.None)]
    public void ValidateCloudImageReference_KnownGuestServicesInjection_ReturnsSuccess(string method)
    {
        var reference = AzureMarketplaceReference();
        reference.GuestServicesInjection = method;

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudImageReference_UnknownGuestServicesInjection_ReturnsFail()
    {
        var reference = AzureMarketplaceReference();
        reference.GuestServicesInjection = "manual";

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudImageReference_ValidAzurePlan_ReturnsSuccess()
    {
        var reference = AzureMarketplaceReference();
        reference.Plan = new AzurePlan
        {
            Publisher = "vendor",
            Product = "vendor-product",
            Name = "byol",
        };

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudImageReference_IncompleteAzurePlan_ReturnsFail()
    {
        var reference = AzureMarketplaceReference();
        reference.Plan = new AzurePlan
        {
            Publisher = "vendor",
            // product missing
            Name = "byol",
        };

        var result = ManifestValidations.ValidateCloudImageReference(reference, "");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudCompatibility_Null_ReturnsSuccess()
    {
        var result = ManifestValidations.ValidateCloudCompatibility(null, "cloud_compatibility");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudCompatibility_ValidMap_ReturnsSuccess()
    {
        var cloudCompatibility = new Dictionary<string, CloudImageReference[]>
        {
            ["sda"] = [AzureMarketplaceReference(), Ec2SsmReference(), Ec2FilterReference()],
        };

        var result = ManifestValidations.ValidateCloudCompatibility(cloudCompatibility, "cloud_compatibility");

        result.Should().BeSuccess();
    }

    [Fact]
    public void ValidateCloudCompatibility_NullEntryArray_ReturnsFail()
    {
        var cloudCompatibility = new Dictionary<string, CloudImageReference[]>
        {
            ["sda"] = null!,
        };

        var result = ManifestValidations.ValidateCloudCompatibility(cloudCompatibility, "cloud_compatibility");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateCloudCompatibility_NullEntry_ReturnsFail()
    {
        var cloudCompatibility = new Dictionary<string, CloudImageReference[]>
        {
            ["sda"] = [null!],
        };

        var result = ManifestValidations.ValidateCloudCompatibility(cloudCompatibility, "cloud_compatibility");

        result.Should().BeFail();
    }

    [Fact]
    public void ValidateGenesetManifest_WithCloudCompatibility_ReturnsSuccess()
    {
        var manifest = new GenesetManifestData
        {
            Geneset = "dbosoft/winsrv2022-standard",
            ShortDescription = "Windows Server 2022 Standard",
            CloudCompatibility = new Dictionary<string, CloudImageReference[]>
            {
                ["sda"] = [AzureMarketplaceReference(), Ec2SsmReference()],
            },
        };

        var result = ManifestValidations.ValidateGenesetManifest(manifest);

        result.Should().BeSuccess();
    }

    [Fact]
    public void GenesetManifest_WithCloudCompatibility_RoundTripsThroughJson()
    {
        const string json = """
            {
              "geneset": "dbosoft/winsrv2022-standard",
              "public": true,
              "short_description": "Windows Server 2022 Standard",
              "cloud_compatibility": {
                "sda": [
                  {
                    "cloud": "azure",
                    "type": "azure_marketplace",
                    "publisher": "MicrosoftWindowsServer",
                    "offer": "WindowsServer",
                    "sku": "2022-datacenter-g2",
                    "version": "latest",
                    "guest_services_injection": "extension"
                  },
                  {
                    "cloud": "ec2",
                    "type": "ec2_ssm_parameter",
                    "parameter": "/aws/service/ami-windows-latest/Windows_Server-2022-English-Full-Base",
                    "guest_services_injection": "geneset"
                  },
                  {
                    "cloud": "ec2",
                    "type": "ec2_image_filter",
                    "owner": "amazon",
                    "name_pattern": "Windows_Server-2022-English-Full-Base-*",
                    "guest_services_injection": "geneset"
                  }
                ]
              }
            }
            """;

        var manifest = JsonSerializer.Deserialize<GenesetManifestData>(
            json, GeneModelDefaults.SerializerOptions);

        manifest.Should().NotBeNull();
        manifest!.CloudCompatibility.Should().ContainKey("sda");
        var entries = manifest.CloudCompatibility!["sda"];
        entries.Should().HaveCount(3);
        entries[0].Type.Should().Be(CloudImageReferenceTypes.AzureMarketplace);
        entries[0].Sku.Should().Be("2022-datacenter-g2");
        entries[0].GuestServicesInjection.Should().Be(GuestServicesInjectionMethods.Extension);
        entries[1].Parameter.Should().NotBeNullOrEmpty();
        entries[2].NamePattern.Should().Be("Windows_Server-2022-English-Full-Base-*");

        ManifestValidations.ValidateGenesetManifest(manifest).Should().BeSuccess();

        // serialize back and confirm snake_case property names are preserved
        var serialized = JsonSerializer.Serialize(manifest, GeneModelDefaults.SerializerOptions);
        serialized.Should().Contain("\"guest_services_injection\"");
        serialized.Should().Contain("\"name_pattern\"");
    }
}
