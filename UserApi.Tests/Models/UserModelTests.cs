using UserApi.Models;
using Xunit;
using FluentAssertions;

namespace UserApi.Tests.Models
{
    public class UserModelTests
    {
        [Fact]
        public void User_Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var user = new User();
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow.AddMinutes(5);

            // Act
            user.Id = 1;
            user.FirstName = "John";
            user.LastName = "Doe";
            user.Email = "john.doe@example.com";
            user.PhoneNumber = "+1234567890";
            user.CreatedAt = createdAt;
            user.UpdatedAt = updatedAt;

            // Assert
            user.Id.Should().Be(1);
            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.Email.Should().Be("john.doe@example.com");
            user.PhoneNumber.Should().Be("+1234567890");
            user.CreatedAt.Should().Be(createdAt);
            user.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void User_WithNullPhoneNumber_ShouldAllowNull()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            user.PhoneNumber.Should().BeNull();
        }

        [Fact]
        public void User_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.Id.Should().Be(0);
            user.FirstName.Should().Be(string.Empty);
            user.LastName.Should().Be(string.Empty);
            user.Email.Should().Be(string.Empty);
            user.PhoneNumber.Should().BeNull();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void User_WithEmptyStrings_ShouldAllowEmptyStrings()
        {
            // Arrange & Act
            var user = new User
            {
                FirstName = "",
                LastName = "",
                Email = "",
                PhoneNumber = ""
            };

            // Assert
            user.FirstName.Should().Be("");
            user.LastName.Should().Be("");
            user.Email.Should().Be("");
            user.PhoneNumber.Should().Be("");
        }
    }
}
