/****************************************************
 * File Name: BackOfficeController.cs
 * Description: Defining Endpoint and Role authentication for Backoffice .
 * Author: Avindi Obeyesekere
 * Last Changes Date: 2025-09-28
 ****************************************************/
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVChargingBackend.Services;
using System.Threading.Tasks;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Backoffice")]
    public class BackofficeController : ControllerBase
    {
        private readonly IUserService _userService;

        public BackofficeController(IUserService userService)
        {
            _userService = userService;
        }

        // Get all EV Owners
        [HttpGet("evowners")]
        public async Task<IActionResult> GetAllEVOwners()
        {
            var evOwners = await _userService.GetUsersByRoleAsync("EVOwner");

            var result = evOwners.Select(u => new UserResponseDto
            {
                Id = u.Id.ToString(),
                Username = u.Username,
                Role = u.Role,
                NIC = u.NIC,
                Active = u.Active
            }).ToList();

            return Ok(result);
        }


        // Get all Station Operators
        [HttpGet("stationoperators")]
        public async Task<IActionResult> GetAllStationOperators()
        {
            var operators = await _userService.GetUsersByRoleAsync("StationOperator");
            return Ok(operators);
        }
    }
}
