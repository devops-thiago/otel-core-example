using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserApi.Controllers;
using UserApi.DTOs;
using UserApi.Services;
using Xunit;
using FluentAssertions;
using AutoFixture;
using System.Diagnostics;

namespace UserApi.Tests.Controllers
{
    public class UserControllerUnitTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;
        private readonly IFixture _fixture;

        public UserControllerUnitTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _controller = new UserController(_mockUserService.Object, _mockLogger.Object);
            _fixture = new Fixture();

            // Configure AutoFixture
            _fixture.Customize<CreateUserDto>(c => c
                .With(x => x.Email, () => _fixture.Create<string>() + "@example.com"));
        }

        [Fact]
        public async Task GetUsers_WhenServiceThrowsException_ShouldReturn500()
        {
            // Arrange
            _mockUserService.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while retrieving users");
        }

        [Fact]
        public async Task GetUser_WhenServiceThrowsException_ShouldReturn500()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while retrieving the user");
        }

        [Fact]
        public async Task CreateUser_WhenServiceThrowsException_ShouldReturn500()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();
            _mockUserService.Setup(x => x.CreateUserAsync(createUserDto))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while creating the user");
        }

        [Fact]
        public async Task CreateUser_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateUser_WhenServiceThrowsInvalidOperationException_ShouldReturnConflict()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();
            _mockUserService.Setup(x => x.CreateUserAsync(createUserDto))
                .ThrowsAsync(new InvalidOperationException("User already exists"));

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("User already exists");
        }

        [Fact]
        public async Task UpdateUser_WhenServiceThrowsException_ShouldReturn500()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while updating the user");
        }

        [Fact]
        public async Task UpdateUser_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_WhenServiceThrowsInvalidOperationException_ShouldReturnConflict()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("Email already exists");
        }

        [Fact]
        public async Task DeleteUser_WhenServiceThrowsException_ShouldReturn500()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while deleting the user");
        }

        [Fact]
        public async Task GetUsers_WithValidRequest_ShouldReturnOk()
        {
            // Arrange
            var users = _fixture.CreateMany<UserResponseDto>(2).ToList();
            _mockUserService.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var userId = 1;
            var user = _fixture.Create<UserResponseDto>();
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto?)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found");
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();
            var createdUser = _fixture.Create<UserResponseDto>();
            _mockUserService.Setup(x => x.CreateUserAsync(createUserDto))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.Value.Should().BeEquivalentTo(createdUser);
            createdResult.ActionName.Should().Be(nameof(UserController.GetUser));
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            var updatedUser = _fixture.Create<UserResponseDto>();
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(updatedUser);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
                .ReturnsAsync((UserResponseDto?)null);

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found");
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found");
        }
    }
}
