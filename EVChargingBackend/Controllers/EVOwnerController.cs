using System.Security.Claims;
using System.Threading.Tasks;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // EVOwner updates their own info using NIC from token
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] User updatedOwner)
        {
            string nic;

            if (User.IsInRole("EVOwner"))
            {
                nic = User.FindFirst("nic")?.Value;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("NIC not found in token.");
            }
            else if (User.IsInRole("Backoffice"))
            {
                nic = updatedOwner.NIC; // Backoffice must send NIC in request body
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("Backoffice must provide NIC in request body.");
            }
            else
            {
                return Forbid();
            }

            var result = await _eVOwnerService.UpdateEVOwnerAsync(nic, updatedOwner);
            return Ok(result);
        }


        //deactivate 
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromBody] NICDto dto = null)
        {
            string nic;

            if (User.IsInRole("EVOwner"))
            {
                // Use NIC from token for EVOwner
                nic = User.FindFirst("nic")?.Value;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("NIC not found in token.");
            }
            else if (User.IsInRole("Backoffice"))
            {
                // Use NIC from request body for Backoffice
                nic = dto?.NIC;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("Backoffice must provide NIC in body.");
            }
            else
            {
                return Forbid();
            }

            var success = await _eVOwnerService.DeactivateEVOwnerAsync(nic);
            return Ok(new { Success = success });
        }

        //activate
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] NICDto dto = null)
        {
            string nic;

            if (User.IsInRole("EVOwner"))
            {
                // Use NIC from token for EVOwner
                nic = User.FindFirst("nic")?.Value;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("NIC not found in token.");
            }
            else if (User.IsInRole("Backoffice"))
            {
                // Use NIC from request body for Backoffice
                nic = dto?.NIC;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("Backoffice must provide NIC in body.");
            }
            else
            {
                return Forbid();
            }

            var success = await _eVOwnerService.ActivateEVOwnerAsync(nic);
            return Ok(new { Success = success });
        }

        // Get EVOwner details by NIC
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpGet("{nic?}")]
        public async Task<IActionResult> GetByNIC(string nic = null)
        {
            // If EVOwner, get NIC from token
            if (User.IsInRole("EVOwner"))
            {
                nic = User.FindFirst("nic")?.Value;
                if (string.IsNullOrEmpty(nic))
                    return BadRequest("NIC not found in token.");
            }
            // Backoffice can supply NIC in route as usual
            else if (User.IsInRole("Backoffice") && string.IsNullOrEmpty(nic))
            {
                return BadRequest("Backoffice must provide NIC.");
            }

            var evoOwner = await _eVOwnerService.GetByNICAsync(nic);
            if (evoOwner == null) return NotFound("EVOwner not found.");
            return Ok(evoOwner);
        }

        //get all evownvers
        [Authorize(Roles = "Backoffice")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllEVOwners()
        {
            var evoOwners = await _eVOwnerService.GetAllEVOwnersAsync();
            return Ok(evoOwners);
        }

    }
}
