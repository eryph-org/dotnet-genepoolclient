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
}
