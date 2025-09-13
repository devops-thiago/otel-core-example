using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UserApi.DTOs;
using UserApi.Services;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private static readonly ActivitySource ActivitySource = new("UserApi");

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            using var activity = ActivitySource.StartActivity("GetUsers");
            activity?.SetTag("operation", "get_all_users");

            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get a user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            using var activity = ActivitySource.StartActivity("GetUser");
            activity?.SetTag("operation", "get_user_by_id");
            activity?.SetTag("user.id", id);

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "User not found");
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user with ID {UserId}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(CreateUserDto createUserDto)
        {
            using var activity = ActivitySource.StartActivity("CreateUser");
            activity?.SetTag("operation", "create_user");
            // Avoid logging PII in traces - use a sanitized version or omit entirely
            activity?.SetTag("user.email.domain", GetEmailDomain(createUserDto.Email));

            try
            {
                if (!ModelState.IsValid)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Invalid model state");
                    return BadRequest(ModelState);
                }

                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                var sanitizedEmail = SanitizeEmailForLogging(createUserDto.Email);
                _logger.LogWarning(ex, "Invalid operation while creating user with email {Email}", sanitizedEmail);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                var sanitizedEmail = SanitizeEmailForLogging(createUserDto.Email);
                _logger.LogError(ex, "Error occurred while creating user with email {Email}", sanitizedEmail);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            using var activity = ActivitySource.StartActivity("UpdateUser");
            activity?.SetTag("operation", "update_user");
            activity?.SetTag("user.id", id);

            try
            {
                if (!ModelState.IsValid)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Invalid model state");
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "User not found");
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating user with ID {UserId}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            using var activity = ActivitySource.StartActivity("DeleteUser");
            activity?.SetTag("operation", "delete_user");
            activity?.SetTag("user.id", id);

            try
            {
                var deleted = await _userService.DeleteUserAsync(id);
                if (!deleted)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "User not found");
                    return NotFound($"User with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        private static string SanitizeEmailForLogging(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return "[empty]";
            }

            var atIndex = email.IndexOf('@');
            if (atIndex < 0)
            {
                return "[invalid]";
            }

            // Keep first character and domain, mask the rest
            var localPart = email.Substring(0, atIndex);
            var domain = email.Substring(atIndex);

            if (localPart.Length <= 1)
            {
                return $"*{domain}";
            }

            return $"{localPart[0]}***{domain}";
        }

        private static string GetEmailDomain(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return "[empty]";
            }

            var atIndex = email.IndexOf('@');
            if (atIndex < 0)
            {
                return "[invalid]";
            }

            return email.Substring(atIndex + 1);
        }
    }
}
