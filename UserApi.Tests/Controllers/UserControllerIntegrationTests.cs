using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using UserApi.Data;
using UserApi.DTOs;
using FluentAssertions;
using AutoFixture;
using UserApi.Tests;

namespace UserApi.Tests.Controllers
{
    public class UserControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>, IDisposable
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly IFixture _fixture;
        private readonly IServiceScope _scope;
        private readonly UserDbContext _context;

        public UserControllerIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _fixture = new Fixture();
            
            // Configure AutoFixture to generate valid email addresses
            _fixture.Customize<CreateUserDto>(c => c
                .With(x => x.Email, _fixture.Create<string>() + "@example.com")
                .With(x => x.FirstName, _fixture.Create<string>().Substring(0, Math.Min(10, _fixture.Create<string>().Length)))
                .With(x => x.LastName, _fixture.Create<string>().Substring(0, Math.Min(10, _fixture.Create<string>().Length))));
                
            _fixture.Customize<UpdateUserDto>(c => c
                .With(x => x.Email, _fixture.Create<string>() + "@example.com")
                .With(x => x.FirstName, _fixture.Create<string>().Substring(0, Math.Min(10, _fixture.Create<string>().Length)))
                .With(x => x.LastName, _fixture.Create<string>().Substring(0, Math.Min(10, _fixture.Create<string>().Length))));

            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<UserDbContext>();
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOkWithUsers()
        {
            // Arrange
            var users = _fixture.CreateMany<User>(2).ToList();
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<UserResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnOkWithUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/user/{user.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(user.FirstName);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.GetAsync($"/api/user/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var createUserDto = _fixture.Create<CreateUserDto>();
            var json = JsonSerializer.Serialize(createUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.FirstName.Should().Be(createUserDto.FirstName);
            result.LastName.Should().Be(createUserDto.LastName);
            result.Email.Should().Be(createUserDto.Email);
        }

        [Fact]
        public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                FirstName = "", // Invalid: empty string
                LastName = "", // Invalid: empty string
                Email = "invalid-email" // Invalid: not a valid email
            };
            var json = JsonSerializer.Serialize(createUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var existingUser = _fixture.Create<User>();
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = _fixture.Build<CreateUserDto>()
                .With(x => x.Email, existingUser.Email)
                .Create();
            var json = JsonSerializer.Serialize(createUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = _fixture.Build<UpdateUserDto>()
                .With(x => x.FirstName, "UpdatedFirstName")
                .With(x => x.LastName, "UpdatedLastName")
                .Create();
            var json = JsonSerializer.Serialize(updateUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/user/{user.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(updateUserDto.FirstName);
            result.LastName.Should().Be(updateUserDto.LastName);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = 999;
            var updateUserDto = _fixture.Create<UpdateUserDto>();
            var json = JsonSerializer.Serialize(updateUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/user/{invalidId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/user/{user.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify user was deleted
            var deletedUser = await _context.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.DeleteAsync($"/api/user/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }

        public void Dispose()
        {
            _client?.Dispose();
            _scope?.Dispose();
            _context?.Dispose();
        }
    }
}
