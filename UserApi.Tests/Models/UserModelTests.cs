using FluentAssertions;
using UserApi.Models;
using Xunit;

namespace UserApi.Tests.Models;

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
        user.Name = "John Doe";
        user.Email = "john.doe@example.com";
        user.Bio = "Software Engineer";
        user.CreatedAt = createdAt;
        user.UpdatedAt = updatedAt;

        // Assert
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john.doe@example.com");
        user.Bio.Should().Be("Software Engineer");
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void User_WithNullBio_ShouldAllowNull()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        user.Bio.Should().BeNull();
    }

    [Fact]
    public void User_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.Bio.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_WithEmptyStrings_ShouldAllowEmptyStrings()
    {
        // Arrange & Act
        var user = new User
        {
            Name = "",
            Email = "",
            Bio = ""
        };

        // Assert
        user.Name.Should().Be("");
        user.Email.Should().Be("");
        user.Bio.Should().Be("");
    }
}
