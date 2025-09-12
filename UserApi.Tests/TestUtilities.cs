using System.Text.Json;
using UserApi.DTOs;

namespace UserApi.Tests
{
    public static class TestUtilities
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static StringContent CreateJsonContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, JsonOptions);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }

        public static CreateUserDto CreateValidUserDto()
        {
            return new CreateUserDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890"
            };
        }

        public static UpdateUserDto CreateValidUpdateUserDto()
        {
            return new UpdateUserDto
            {
                FirstName = "Updated",
                LastName = "User",
                Email = "updated@example.com",
                PhoneNumber = "+0987654321"
            };
        }
    }
}
