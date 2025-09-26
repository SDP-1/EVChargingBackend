using BCrypt.Net;  // Correct namespace

namespace EVChargingBackend.Controllers
{
    using System.Threading.Tasks;
    using EVChargingBackend.Helpers;
    using EVChargingBackend.Models;    // Ensure the User model is being used
    using EVChargingBackend.Services;  // Ensure that IUserService is being used correctly
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
            // Validate NIC for EVOwner role
            if (user.Role == "EVOwner" && string.IsNullOrEmpty(user.NIC))
                return BadRequest("NIC is required for EVOwner.");

            // Hash the password before storing
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Create a new user in MongoDB
            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(new { Username = createdUser.Username, Role = createdUser.Role, UserId = createdUser.Id.ToString() });
        }


        // User Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Get the user by username
            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate the JWT token with userId included
            var token = JwtHelper.GenerateJwtToken(
                user.Username,
                user.Role,
                user.Id.ToString(),                    // Pass the MongoDB ObjectId
                _configuration["Jwt:SecretKey"]
            );

            return Ok(new
            {
                Token = token
            });
        }
    }
}
