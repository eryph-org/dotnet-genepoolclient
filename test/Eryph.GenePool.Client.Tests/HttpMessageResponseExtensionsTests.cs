using System.Net;
using System.Text.Json;
using Azure;
using Azure.Core;
using Eryph.GenePool.Client.Internal;
using Eryph.GenePool.Client.Responses;
using Moq;
using System.Text.Json.Serialization;
using FluentAssertions;

namespace Eryph.GenePool.Client.Tests;

public class HttpMessageResponseExtensionsTests
{
    [Fact]
    public void DeserializeResponse_OkWithValidJson_ReturnsResult()
    {
        var response = new MockResponse(200)
            .WithJson("""
                      {
                        "name": "John Doe",
                        "age": 42
                      }
                      """);

        var message = CreateMessage(response);
        var result = message.DeserializeResponse<Customer>();

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Age.Should().Be(42);
    }

    [Fact]
    public async Task DeserializeResponseAsync_OkWithValidJson_ReturnsResult()
    {
        var response = new MockResponse(200)
            .WithJson("""
                      {
                        "name": "John Doe",
                        "age": 42
                      }
                      """);
        
        var message = CreateMessage(response);
        var result = await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);
        
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Age.Should().Be(42);
    }

    [Fact]
    public void DeserializeResponse_OkWithInvalidJson_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithJson("""
                      {
                        "name": "John Doe",
                        "age": "not-a-number"
                      }
                      """);

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The JSON response is not a valid Customer*")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public async Task DeserializeResponseAsync_OkWithInvalidJson_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithJson("""
                      {
                        "name": "John Doe",
                        "age": "not-a-number"
                      }
                      """);

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The JSON response is not a valid Customer*")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public void DeserializeResponse_OkWithCorruptJson_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithJson("{");

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The response does not contain valid JSON: *")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public async Task DeserializeResponseAsync_OkWithCorruptJson_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithJson("{");

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The response does not contain valid JSON: *")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public void DeserializeResponse_OkWithHtml_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithHtml("<html><body><h1>Hello, World!</h1></body></html>");

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The content type of the response is invalid: text/html; charset=utf-8")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeserializeResponseAsync_OkWithHtml_ThrowsException()
    {
        var response = new MockResponse(200)
            .WithHtml("<html><body><h1>Hello, World!</h1></body></html>");

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The content type of the response is invalid: text/html; charset=utf-8")
            .Which.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void DeserializeResponse_BadRequestWithValidJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("""
                      {
                        "status_code": 400,
                        "message": "test-error-message"
                      }
                      """);

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<ErrorResponseException>();
        exception.WithMessage("test-error-message")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = exception.Which.Response.Should().BeOfType<ErrorResponse>().Subject;
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("test-error-message");
    }

    [Fact]
    public async Task DeserializeResponseAsync_BadRequestWithValidJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("""
                      {
                        "status_code": 400,
                        "message": "test-error-message"
                      }
                      """);

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ErrorResponseException>();
        exception.WithMessage("test-error-message")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = exception.Which.Response.Should().BeOfType<ErrorResponse>().Subject;
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("test-error-message");
    }

    [Fact]
    public void DeserializeResponse_BadRequestWithInvalidJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("""
                      {
                        "message": 42
                      }
                      """);

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The JSON response is not a valid ErrorResponse*")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public async Task DeserializeResponseAsync_BadRequestWithInvalidJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("""
                      {
                        "message": 42
                      }
                      """);

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The JSON response is not a valid ErrorResponse*")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public void DeserializeResponse_BadRequestWithCorruptJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("{");

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The response does not contain valid JSON: *")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public async Task DeserializeResponseAsync_BadRequestWithCorruptJson_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithJson("{");

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The response does not contain valid JSON: *")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.WithInnerException<JsonException>();
    }

    [Fact]
    public void DeserializeResponse_BadRequestWithHtml_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithHtml("<html><body><h1>Error!</h1></body></html>");

        var message = CreateMessage(response);
        var act = () => message.DeserializeResponse<Customer>();

        var exception = act.Should().Throw<GenepoolClientException>();
        exception.WithMessage("The content type of the response is invalid: text/html; charset=utf-8")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeserializeResponseAsync_BadRequestWithHtml_ThrowsException()
    {
        var response = new MockResponse(400)
            .WithIsError(true)
            .WithHtml("<html><body><h1>Error!</h1></body></html>");

        var message = CreateMessage(response);
        var act = async () => await message.DeserializeResponseAsync<Customer>(
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GenepoolClientException>();
        exception.WithMessage("The content type of the response is invalid: text/html; charset=utf-8")
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static HttpMessage CreateMessage(Response response)
    {
        var message =  new HttpMessage(
            Mock.Of<Request>(),
            Mock.Of<ResponseClassifier>());
        message.Response = response;
        
        return message;
    }

    private sealed record Customer : ResponseBase
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }
    }
}
