using BCrypt.Net;  // Correct namespace

namespace EVChargingBackend.Controllers
{
    using System.Threading.Tasks;
    using EVChargingBackend.Helpers;
    using EVChargingBackend.Models;    // Ensure the User model is being used
    using EVChargingBackend.Services;  // Ensure that IUserService is being used correctly
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // User Registration endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Normalize role
            var role = user.Role?.Trim();

            // Validate role
            if (role != "Backoffice" && role != "StationOperator" && role != "EVOwner")
                return BadRequest("Invalid role. Must be Backoffice, StationOperator, or EVOwner.");

            // NIC required only for EVOwner
            if (role == "EVOwner" && string.IsNullOrEmpty(user.NIC))
                return BadRequest("NIC is required for EVOwner.");

            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Set normalized role
            user.Role = role;

            // Set Active based on role
            user.Active = role == "Backoffice";  // Backoffice = true, others = false

            // Ensure ID is null so MongoDB can auto-generate it
            user.Id = null;

            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(new { Username = createdUser.Username, Role = createdUser.Role, UserId = createdUser.Id?.ToString(), Active = createdUser.Active });
        }



        // User Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                return Unauthorized(new { Code = "USER_NOT_FOUND", Message = "User does not exist." });
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { Code = "INVALID_PASSWORD", Message = "Incorrect password." });
            }

            // Only allow login if user is active, unless Backoffice
            //if (user.Role != "Backoffice" && !user.Active)
            if (!user.Active)
            {
                return Unauthorized(new { Code = "INACTIVE_ACCOUNT", Message = "Account is not active. Contact Backoffice." });
            }

            var token = JwtHelper.GenerateJwtToken(
                user.Username,
                user.Role,
                user.Id.ToString(),
                user.NIC,
                _configuration["Jwt:SecretKey"]
            );

            return Ok(new { Token = token });
        }

        // Activate a EVOwner or StationOperator
        [Authorize(Roles = "Backoffice")]
        [HttpPost("activate/{userId}")]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required.");

            var updatedUser = await _userService.SetUserActiveStatusAsync(userId, true);
            if (updatedUser == null) return NotFound("User not found.");

            return Ok(updatedUser); // Return full updated user
        }

        // Deactivate a EVOwner or StationOperator
        [Authorize(Roles = "Backoffice")]
        [HttpPost("deactivate/{userId}")]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required.");

            var updatedUser = await _userService.SetUserActiveStatusAsync(userId, false);
            if (updatedUser == null) return NotFound("User not found.");

            return Ok(updatedUser); // Return full updated user
        }

    }
}
