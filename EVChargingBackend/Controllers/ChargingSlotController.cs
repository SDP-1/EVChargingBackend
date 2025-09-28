using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingSlotController : ControllerBase
    {
        private readonly IChargingSlotService _slotService;

        public ChargingSlotController(IChargingSlotService slotService)
        {
            _slotService = slotService;
        }

        // Initialize daily slots (Backoffice only)
        [Authorize(Roles = "Backoffice")]
        [HttpPost("init/{stationId}/{date}")]
        public async Task<IActionResult> InitializeSlots(string stationId, DateTime date)
        {
            await _slotService.InitializeDailySlotsAsync(ObjectId.Parse(stationId), date);
            return Ok("Daily slots initialized.");
        }

        // Get available slots (EV Owner)
        [Authorize(Roles = "EVOwner")]
        [HttpGet("available/{stationId}/{date}")]
        public async Task<IActionResult> GetAvailableSlots(string stationId, DateTime date)
        {
            var slots = await _slotService.GetAvailableSlotsAsync(ObjectId.Parse(stationId), date);
            return Ok(slots);
        }

        // Book a slot (EV Owner)
        [Authorize(Roles = "EVOwner")]
        [HttpPost("book/{slotId}/{bookingId}")]
        public async Task<IActionResult> BookSlot(string slotId, string bookingId)
        {
            var evoOwnerId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(evoOwnerId)) return BadRequest("UserId not found in token.");

            var success = await _slotService.BookSlotAsync(ObjectId.Parse(slotId), evoOwnerId, bookingId);
            return Ok(new { Success = success });
        }
    }
}
