using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UserApi.DTOs;
using Xunit;

namespace UserApi.Tests.DTOs;

public class UserDtoValidationTests
{
    [Fact]
    public void CreateUserDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "Software Engineer"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateUserDto_WithInvalidName_ShouldFailValidation(string name)
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = name,
            Email = "john.doe@example.com",
            Bio = "Engineer"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CreateUserDto_WithNullBio_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = null
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void CreateUserDto_WithInvalidEmail_ShouldFailValidation(string email)
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John Doe",
            Email = email,
            Bio = "Engineer"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CreateUserDto_WithNullName_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = null!,
            Email = "john.doe@example.com",
            Bio = "Engineer"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CreateUserDto_WithNullEmail_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John Doe",
            Email = null!,
            Bio = "Engineer"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
    }

    [Fact]
    public void UpdateUserDto_WithAllNullValues_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Name = null,
            Email = null,
            Bio = null
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void UpdateUserDto_WithValidPartialData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Name = "Updated John",
            Email = "updated.john@example.com",
            Bio = null
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void UpdateUserDto_WithEmptyName_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Name = "",
            Email = "john.doe@example.com"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void UpdateUserDto_WithEmptyBio_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Name = "John Doe",
            Bio = "",
            Email = "john.doe@example.com"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void UpdateUserDto_WithInvalidEmail_ShouldFailValidation(string email)
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Name = "John Doe",
            Email = email
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
    }

    [Fact]
    public void UserResponseDto_Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var dto = new UserResponseDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("John Doe");
        dto.Email.Should().Be("john.doe@example.com");
        dto.Bio.Should().Be("Software Engineer");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
