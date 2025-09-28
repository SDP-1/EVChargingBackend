using AutoMapper;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Backoffice")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // GET: api/User/all - Retrieves all users
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var responseDtos = _mapper.Map<List<UserResponseDto>>(users);
            return Ok(responseDtos);
        }

        // GET: api/User/count - Retrieves total user count
        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await _userService.GetUserCountAsync();
            return Ok(new { TotalUsers = count });
        }

        // GET: api/User/role/{role} - Retrieves users filtered by role
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            // Simple role validation for safety
            var normalizedRole = role.Trim();
            if (normalizedRole != "Backoffice" && normalizedRole != "StationOperator" && normalizedRole != "EVOwner")
                return BadRequest("Invalid role filter.");

            var users = await _userService.GetUsersByRoleAsync(normalizedRole);
            var responseDtos = _mapper.Map<List<UserResponseDto>>(users);
            return Ok(responseDtos);
        }

        // GET: api/User/{id} - Retrieve single user details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var responseDto = _mapper.Map<UserResponseDto>(user);
            return Ok(responseDto);
        }

        // PUT: api/User/{id} - Edit (update) user details
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUserDetails(string id, [FromBody] UserUpdateDto updateDto)
        {
            // Ensure user exists before attempting to update
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Attempt the update
            var success = await _userService.UpdateUserDetailsAsync(id, updateDto);

            if (success)
            {
                // Retrieve and return the updated user object
                var updatedUser = await _userService.GetUserByIdAsync(id);
                var responseDto = _mapper.Map<UserResponseDto>(updatedUser);
                return Ok(responseDto);
            }

            // This case handles when the ID is valid but the service fails to update (e.g., MongoDB error)
            return StatusCode(500, new { Message = "Failed to apply user update." });
        }
    }
}
