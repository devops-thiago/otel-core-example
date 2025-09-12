

using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(UserDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Getting all users");
            
            var users = await _context.Users.ToListAsync();
            
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);
            
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return null;
            }

            return MapToResponseDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            _logger.LogInformation("Creating new user with email: {Email}", createUserDto.Email);
            
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", createUserDto.Email);
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
            
            return MapToResponseDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update", id);
                return null;
            }

            // Check if email is being changed and if it already exists
            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == updateUserDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", updateUserDto.Email);
                    throw new InvalidOperationException("A user with this email already exists.");
                }
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
            {
                user.FirstName = updateUserDto.FirstName;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.LastName))
            {
                user.LastName = updateUserDto.LastName;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                user.Email = updateUserDto.Email;
            }
            
            if (updateUserDto.PhoneNumber != null)
            {
                user.PhoneNumber = updateUserDto.PhoneNumber;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} updated successfully", id);
            
            return MapToResponseDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully", id);
            
            return true;
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
