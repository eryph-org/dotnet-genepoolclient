
using FluentAssertions;
using FluentAssertions.LanguageExt;

namespace Eryph.GenePool.Model.Tests
{
    public class ValidationsTests
    {
        [Theory]
        [InlineData("abc")]
        [InlineData("MTIzNDU2Nzg5MA==")]
        [InlineData(" test")]
        [InlineData("QjYwMUY5N0YtMTRDNS00NDU4LTkxMjAtMjdFRjJBQjNFMjFF")]
        [InlineData("HsIOvfRRrUKTz2lcH4bOSw-")]
        public void ValidateKeyIdString_InvalidKey_ReturnsFail(string value)
        {
            var result = Validations.ValidateKeyIdString(value, "value");

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be("value does not meet the requirements. Value has to be a key id."));
        }

        [Theory]
        [InlineData("HsIOvfRRrUKTz2lcH4bOSw")]
        [InlineData("HsIOvfRRrUKTz2lcH4bOSw==")]
        public void ValidateKeyIdString_ValidKey_ReturnsSuccess(string value)
        {
            var result = Validations.ValidateKeyIdString(value, "value");

            result.Should().BeSuccess();
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("1d.")]
        [InlineData("1ff.00")]
        public void ValidateVersion_InvalidString_ReturnsFail(string value)
        {
            var result = Validations.ValidateVersionString(value);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be($"'{value}' is not a valid version"));
        }

        [Fact]
        public void ValidateGenesetShortDescription_InvalidString_ReturnsFail()
        {
            var chars = new string('a', 91);
            var result = Validations.ValidateGenesetShortDescription(chars);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be("Short description is too long (max. 90 chars allowed)."));
        }

        [Fact]
        public void ValidateGenesetDescription_InvalidString_ReturnsFail()
        {
            var chars = new string('a', 201);
            var result = Validations.ValidateGenesetDescription(chars);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be("Description is too long (max. 200 chars). Use markdown description for longer content."));
        }

        [Fact]
        public void ValidateMarkdownContentSize_ToLong_ReturnsFail()
        {
            var chars = new string('a', GeneModelDefaults.MaxGenesetMarkdownBytes+1);
            var result = Validations.ValidateMarkdownContentSize(chars);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be($"Markdown description is too large. Max size is {GeneModelDefaults.MaxGenesetMarkdownBytes} bytes."));
        }

        [Fact]
        public void ValidateYamlContentSize_ToLong_ReturnsFail()
        {
            var chars = new string('a', GeneModelDefaults.MaxYamlSourceBytes + 1);
            var result = Validations.ValidateYamlContentSize(chars);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be($"YAML content is too large. Max size is {GeneModelDefaults.MaxYamlSourceBytes} bytes."));
        }

        [Fact]
        public void ValidateMetadata_ToManyKeys_ReturnsFail()
        {
            var keys = new Dictionary<string, string>();
            Enumerable.Range(0, GeneModelDefaults.MaxMetadataKeyCount + 1)
                .ToList()
                .ForEach(i => keys.Add(i.ToString(), i.ToString()));

            var result = Validations.ValidateMetadata(keys);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    .Be($"Metadata is too large. Only up to {GeneModelDefaults.MaxMetadataKeyCount} keys are allowed."));
        }

        [Fact]
        public void ValidateMetadata_ToLongKey_ReturnsFail()
        {
            var keys = new Dictionary<string, string>();
            var key = new string('a', GeneModelDefaults.MaxMetadataKeyLength+1);
            keys.Add(key, "a");

            var result = Validations.ValidateMetadata(keys);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    // ReSharper disable once StringLiteralTypo
                    .Be($"Metadata key 'aaaaaaaaaa..' is too long. Max length of keys is {GeneModelDefaults.MaxMetadataKeyLength}."));
        }

        [Fact]
        public void ValidateMetadata_ToLongValue_ReturnsFail()
        {
            var keys = new Dictionary<string, string>();
            var value = new string('a', GeneModelDefaults.MaxMetadataValueLength + 1);
            keys.Add("key", value);

            var result = Validations.ValidateMetadata(keys);

            result.Should().BeFail().Which.Should().SatisfyRespectively(
                error => error.Message.Should()
                    // ReSharper disable once StringLiteralTypo
                    .Be($"Metadata value for key 'key' is too long. Max length of values is {GeneModelDefaults.MaxMetadataValueLength}."));
        }
    }
}