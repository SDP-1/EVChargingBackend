using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVOwnerController : ControllerBase
    {
        private readonly IEVOwnerService _eVOwnerService;

        public EVOwnerController(IEVOwnerService eVOwnerService)
        {
            _eVOwnerService = eVOwnerService;
        }

        // EVOwner updates their own info
        [Authorize(Roles = "EVOwner")]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] User updatedOwner)
        {
            // Extract NIC from token or request body
            var nic = updatedOwner.NIC;
            if (string.IsNullOrEmpty(nic)) return BadRequest("NIC is required.");

            var result = await _eVOwnerService.UpdateEVOwnerAsync(nic, updatedOwner);
            return Ok(result);
        }

        // EVOwner deactivates their own account
        [Authorize(Roles = "EVOwner")]
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromBody] string nic)
        {
            var success = await _eVOwnerService.DeactivateEVOwnerAsync(nic);
            return Ok(new { Success = success });
        }

        // Backoffice reactivates EVOwner account
        [Authorize(Roles = "Backoffice")]
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] string nic)
        {
            var success = await _eVOwnerService.ActivateEVOwnerAsync(nic);
            return Ok(new { Success = success });
        }

        // Get EVOwner details by NIC
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpGet("{nic}")]
        public async Task<IActionResult> GetByNIC(string nic)
        {
            var evoOwner = await _eVOwnerService.GetByNICAsync(nic);
            if (evoOwner == null) return NotFound("EVOwner not found.");
            return Ok(evoOwner);
        }
    }
}
