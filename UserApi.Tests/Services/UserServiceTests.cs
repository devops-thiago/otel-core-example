using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserApi.Data;
using UserApi.DTOs;
using UserApi.Models;
using UserApi.Services;
using Xunit;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;

namespace UserApi.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;
        private readonly IFixture _fixture;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(options);
            _mockLogger = new Mock<ILogger<UserService>>();
            _userService = new UserService(_context, _mockLogger.Object);
            _fixture = new Fixture();

            // Configure AutoFixture to generate proper dates
            _fixture.Customize<User>(c => c
                .With(x => x.CreatedAt, () => DateTime.UtcNow.AddMinutes(-10)) // Past date
                .With(x => x.UpdatedAt, () => DateTime.UtcNow.AddMinutes(-5))  // Past date, but after CreatedAt
                .Without(x => x.Id));
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = _fixture.CreateMany<User>(3).ToList();
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(user =>
            {
                user.Id.Should().BeGreaterThan(0);
                user.FirstName.Should().NotBeNullOrEmpty();
                user.LastName.Should().NotBeNullOrEmpty();
                user.Email.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var result = await _userService.GetUserByIdAsync(invalidId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.FirstName.Should().Be(createUserDto.FirstName);
            result.LastName.Should().Be(createUserDto.LastName);
            result.Email.Should().Be(createUserDto.Email);
            result.PhoneNumber.Should().Be(createUserDto.PhoneNumber);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            // Verify user was saved to database
            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var existingUser = _fixture.Create<User>();
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = _fixture.Build<CreateUserDto>()
                .With(x => x.Email, existingUser.Email)
                .Create();

            // Act & Assert
            await _userService.Invoking(x => x.CreateUserAsync(createUserDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this email already exists.");
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = _fixture.Build<UpdateUserDto>()
                .With(x => x.FirstName, "UpdatedFirstName")
                .With(x => x.LastName, "UpdatedLastName")
                .With(x => x.Email, "updated@example.com")
                .With(x => x.PhoneNumber, "+9999999999")
                .Create();

            var beforeUpdate = DateTime.UtcNow;

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(updateUserDto.FirstName);
            result.LastName.Should().Be(updateUserDto.LastName);
            result.Email.Should().Be(updateUserDto.Email);
            result.PhoneNumber.Should().Be(updateUserDto.PhoneNumber);
            result.UpdatedAt.Should().BeAfter(beforeUpdate.AddSeconds(-1)); // Allow for some time variance
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = 999;
            var updateUserDto = _fixture.Create<UpdateUserDto>();

            // Act
            var result = await _userService.UpdateUserAsync(invalidId, updateUserDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user1 = _fixture.Create<User>();
            var user2 = _fixture.Create<User>();
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var updateUserDto = _fixture.Build<UpdateUserDto>()
                .With(x => x.Email, user2.Email)
                .Create();

            // Act & Assert
            await _userService.Invoking(x => x.UpdateUserAsync(user1.Id, updateUserDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this email already exists.");
        }

        [Fact]
        public async Task UpdateUserAsync_WithPartialData_ShouldUpdateOnlyProvidedFields()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedFirstName"
                // Other fields are null
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(updateUserDto.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Email.Should().Be(user.Email);
            result.PhoneNumber.Should().Be(user.PhoneNumber);
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_ShouldDeleteUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.DeleteUserAsync(user.Id);

            // Assert
            result.Should().BeTrue();

            // Verify user was deleted from database
            var deletedUser = await _context.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUserAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var result = await _userService.DeleteUserAsync(invalidId);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [AutoData]
        public async Task CreateUserAsync_WithAutoFixtureData_ShouldCreateUser(CreateUserDto createUserDto)
        {
            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.FirstName.Should().Be(createUserDto.FirstName);
            result.LastName.Should().Be(createUserDto.LastName);
            result.Email.Should().Be(createUserDto.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_WithEmptyEmail_ShouldUpdateWithoutEmailChange()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedFirstName",
                Email = ""
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(updateUserDto.FirstName);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullPhoneNumber_ShouldKeepExistingPhoneNumber()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var originalPhoneNumber = user.PhoneNumber;

            var updateUserDto = new UpdateUserDto
            {
                PhoneNumber = null
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.PhoneNumber.Should().Be(originalPhoneNumber);
        }

        [Fact]
        public async Task UpdateUserAsync_WithSameEmail_ShouldNotThrowException()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                Email = user.Email // Same email should be allowed
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        public async Task UpdateUserAsync_WithEmptyFirstName_ShouldNotUpdateFirstName(string firstName)
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = firstName,
                LastName = "UpdatedLastName"
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(updateUserDto.LastName);
        }

        [Theory]
        [InlineData("")]
        public async Task UpdateUserAsync_WithEmptyLastName_ShouldNotUpdateLastName(string lastName)
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedFirstName",
                LastName = lastName
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(updateUserDto.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullFirstName_ShouldNotUpdateFirstName()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = null,
                LastName = "UpdatedLastName"
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(updateUserDto.LastName);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullLastName_ShouldNotUpdateLastName()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedFirstName",
                LastName = null
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateUserDto);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(updateUserDto.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
