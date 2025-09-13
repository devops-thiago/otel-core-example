using System.ComponentModel.DataAnnotations;
using UserApi.DTOs;
using Xunit;
using FluentAssertions;

namespace UserApi.Tests.DTOs
{
    public class UserDtoValidationTests
    {
        [Fact]
        public void CreateUserDto_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateUserDto_WithInvalidFirstName_ShouldFailValidation(string firstName)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = firstName,
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateUserDto_WithInvalidLastName_ShouldFailValidation(string lastName)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = "John",
                LastName = lastName,
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("LastName"));
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
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateUserDto_WithNullFirstName_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = null!,
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void CreateUserDto_WithNullLastName_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = "John",
                LastName = null!,
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void CreateUserDto_WithNullEmail_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = null!,
                PhoneNumber = "+1234567890"
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
                FirstName = null,
                LastName = null,
                Email = null,
                PhoneNumber = null
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
                FirstName = "UpdatedJohn",
                LastName = null,
                Email = "updated.john@example.com",
                PhoneNumber = null
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void UpdateUserDto_WithEmptyFirstName_ShouldPassValidation()
        {
            // Arrange
            var dto = new UpdateUserDto
            {
                FirstName = "",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void UpdateUserDto_WithEmptyLastName_ShouldPassValidation()
        {
            // Arrange
            var dto = new UpdateUserDto
            {
                FirstName = "John",
                LastName = "",
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
                FirstName = "John",
                LastName = "Doe",
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
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            dto.Id.Should().Be(1);
            dto.FirstName.Should().Be("John");
            dto.LastName.Should().Be("Doe");
            dto.Email.Should().Be("john.doe@example.com");
            dto.PhoneNumber.Should().Be("+1234567890");
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
}
