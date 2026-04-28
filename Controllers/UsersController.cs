using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    /// <summary>
    /// User Management API — CRUD endpoints for managing TechHive Solutions users.
    /// All endpoints require a valid Bearer token in the Authorization header.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger      = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/users
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Retrieve all users.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");
                var users = await _userService.GetAllUsersAsync();
                return Ok(ApiResponse<IEnumerable<User>>.Ok(users, $"{users.Count()} user(s) found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users.");
                throw; // Propagate to ErrorHandlingMiddleware
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/users/{id}
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Retrieve a specific user by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID {Id}.", id);

                if (id <= 0)
                    return BadRequest(ApiResponse<object>.Fail("ID must be a positive integer."));

                var user = await _userService.GetUserByIdAsync(id);

                if (user is null)
                {
                    _logger.LogWarning("User with ID {Id} not found.", id);
                    return NotFound(ApiResponse<object>.Fail($"User with ID {id} was not found."));
                }

                return Ok(ApiResponse<User>.Ok(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {Id}.", id);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/users
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Create a new user.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                // ModelState validation (data annotations on DTO)
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
                }

                // Check for duplicate email
                if (await _userService.EmailExistsAsync(dto.Email))
                {
                    return Conflict(ApiResponse<object>.Fail(
                        $"A user with the email '{dto.Email}' already exists."));
                }

                _logger.LogInformation("Creating new user with email {Email}.", dto.Email);
                var created = await _userService.CreateUserAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    ApiResponse<User>.Ok(created, "User created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/users/{id}
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Update an existing user by ID.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<object>.Fail("ID must be a positive integer."));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
                }

                // Check email conflict (exclude current user)
                if (await _userService.EmailExistsAsync(dto.Email, excludeId: id))
                {
                    return Conflict(ApiResponse<object>.Fail(
                        $"A different user already uses the email '{dto.Email}'."));
                }

                _logger.LogInformation("Updating user with ID {Id}.", id);
                var updated = await _userService.UpdateUserAsync(id, dto);

                if (updated is null)
                {
                    _logger.LogWarning("Update failed — user ID {Id} not found.", id);
                    return NotFound(ApiResponse<object>.Fail($"User with ID {id} was not found."));
                }

                return Ok(ApiResponse<User>.Ok(updated, "User updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}.", id);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/users/{id}
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Delete a user by ID.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<object>.Fail("ID must be a positive integer."));

                _logger.LogInformation("Deleting user with ID {Id}.", id);
                var deleted = await _userService.DeleteUserAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Delete failed — user ID {Id} not found.", id);
                    return NotFound(ApiResponse<object>.Fail($"User with ID {id} was not found."));
                }

                return Ok(ApiResponse<string>.Ok(string.Empty, $"User with ID {id} deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}.", id);
                throw;
            }
        }
    }
}