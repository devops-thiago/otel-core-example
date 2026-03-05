using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using UserApi.Infrastructure;
using Xunit;

namespace UserApi.Tests.Infrastructure;

public class CompatibleSystemTextJsonOutputFormatterTests
{
    [Fact]
    public void Constructor_WithOptions_ShouldSupportJsonMediaTypes()
    {
        // Arrange & Act
        var formatter = new CompatibleSystemTextJsonOutputFormatter(new JsonSerializerOptions());

        // Assert
        formatter.SupportedMediaTypes.Should().Contain("application/json");
        formatter.SupportedMediaTypes.Should().Contain("text/json");
        formatter.SupportedMediaTypes.Should().Contain("application/*+json");
    }

    [Fact]
    public void Constructor_WithOptions_ShouldSupportEncodings()
    {
        // Arrange & Act
        var formatter = new CompatibleSystemTextJsonOutputFormatter(new JsonSerializerOptions());

        // Assert
        formatter.SupportedEncodings.Should().Contain(Encoding.UTF8);
        formatter.SupportedEncodings.Should().Contain(Encoding.Unicode);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaults()
    {
        // Arrange & Act
        var formatter = new CompatibleSystemTextJsonOutputFormatter(null!);

        // Assert
        formatter.SupportedMediaTypes.Should().Contain("application/json");
    }

    [Fact]
    public async Task WriteResponseBodyAsync_ShouldSerializeObject()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var formatter = new CompatibleSystemTextJsonOutputFormatter(options);

        var testObject = new { Name = "John", Email = "john@example.com" };
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            testObject.GetType(),
            testObject);

        // Act
        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("\"name\"");
        json.Should().Contain("\"email\"");
        json.Should().Contain("John");
        json.Should().Contain("john@example.com");
    }

    [Fact]
    public async Task WriteResponseBodyAsync_WithNullObject_ShouldWriteNull()
    {
        // Arrange
        var formatter = new CompatibleSystemTextJsonOutputFormatter(new JsonSerializerOptions());

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(object),
            null);

        // Act
        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        json.Should().Be("null");
    }

    [Fact]
    public async Task WriteResponseBodyAsync_WithUnicodeEncoding_ShouldWork()
    {
        // Arrange
        var formatter = new CompatibleSystemTextJsonOutputFormatter(new JsonSerializerOptions());

        var testObject = new { Message = "Hello World" };
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            testObject.GetType(),
            testObject);

        // Act
        await formatter.WriteResponseBodyAsync(context, Encoding.Unicode);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.Unicode);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("Hello World");
    }
}
