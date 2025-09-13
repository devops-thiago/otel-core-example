using UserApi.Controllers;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace UserApi.Tests.Controllers
{
    public class UserControllerHelperMethodsTests
    {
        [Theory]
        [InlineData("test@example.com", "t***@example.com")]
        [InlineData("a@example.com", "*@example.com")]
        [InlineData("", "[empty]")]
        [InlineData("invalid-email", "[invalid]")]
        [InlineData("@example.com", "*@example.com")]
        [InlineData("test", "[invalid]")]
        public void SanitizeEmailForLogging_WithVariousInputs_ShouldReturnExpectedResults(string input, string expected)
        {
            // Act
            var result = InvokeSanitizeEmailForLogging(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("test@example.com", "example.com")]
        [InlineData("user@domain.org", "domain.org")]
        [InlineData("", "[empty]")]
        [InlineData("invalid-email", "[invalid]")]
        [InlineData("test@", "")]
        [InlineData("@domain.com", "domain.com")]
        public void GetEmailDomain_WithVariousInputs_ShouldReturnExpectedResults(string input, string expected)
        {
            // Act
            var result = InvokeGetEmailDomain(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("ab@example.com", "a***@example.com")]
        [InlineData("abc@example.com", "a***@example.com")]
        [InlineData("abcdef@example.com", "a***@example.com")]
        [InlineData("verylongemail@example.com", "v***@example.com")]
        public void SanitizeEmailForLogging_WithDifferentLengths_ShouldAlwaysShowFirstCharAndDomain(string input, string expected)
        {
            // Act
            var result = InvokeSanitizeEmailForLogging(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("test@sub.domain.com", "sub.domain.com")]
        [InlineData("user@localhost", "localhost")]
        [InlineData("admin@127.0.0.1", "127.0.0.1")]
        public void GetEmailDomain_WithComplexDomains_ShouldReturnFullDomain(string input, string expected)
        {
            // Act
            var result = InvokeGetEmailDomain(input);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void SanitizeEmailForLogging_WithNullInput_ShouldReturnEmpty()
        {
            // Act
            var result = InvokeSanitizeEmailForLogging(null);

            // Assert
            result.Should().Be("[empty]");
        }

        [Fact]
        public void GetEmailDomain_WithNullInput_ShouldReturnEmpty()
        {
            // Act
            var result = InvokeGetEmailDomain(null);

            // Assert
            result.Should().Be("[empty]");
        }

        private static string InvokeSanitizeEmailForLogging(string? email)
        {
            var method = typeof(UserController).GetMethod("SanitizeEmailForLogging",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method!.Invoke(null, new object?[] { email })!;
        }

        private static string InvokeGetEmailDomain(string? email)
        {
            var method = typeof(UserController).GetMethod("GetEmailDomain",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method!.Invoke(null, new object?[] { email })!;
        }
    }
}
