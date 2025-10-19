/****************************************************
 * File Name: ChargingSlotController.cs
 * Description: Defining Endpoint and Role authentication for ChargingSlots .
 * Author: Avindi Obeyesekere
 * Last Changes Date: 2025-10-05
 ****************************************************/
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            await _slotService.InitializeDailySlotsAsync(stationId, date);
            return Ok("Daily slots initialized.");
        }

        // Get available slots (EV Owner)
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpGet("available/{stationId}/{date}")]
        public async Task<IActionResult> GetAvailableSlots(string stationId, DateTime date)
        {
            var slots = await _slotService.GetAvailableSlotsAsync(stationId, date);
            return Ok(slots);
        }

        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpGet("all/{stationId}/{date}")]
        public async Task<IActionResult> GetAllSlots(string stationId, DateTime date)
        {
            var slots = await _slotService.GetAllSlotsAsync(stationId, date); // Returns all slots regardless of IsBooked
            return Ok(slots);
        }

        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpGet("booked/{stationId}/{date}")]
        public async Task<IActionResult> GetBookedSlots(string stationId, DateTime date)
        {
            var slots = await _slotService.GetBookedSlotsAsync(stationId, date);
            return Ok(slots);
        }

        // Get a slot by its Id
        [Authorize(Roles = "EVOwner,Backoffice")]
        [HttpGet("{slotId}")]
        public async Task<IActionResult> GetSlotById(string slotId)
        {
            var slot = await _slotService.GetSlotByIdAsync(slotId);
            if (slot == null) return NotFound("Slot not found.");
            return Ok(slot);
        }

        [Authorize(Roles = "Backoffice")]
        [HttpDelete("deinit/{stationId}/{date}")]
        public async Task<IActionResult> DeinitializeSlots(string stationId, DateTime date)
        {
            var success = await _slotService.DeleteSlotsForDateAsync(stationId, date);
            if (!success)
                return NotFound("No slots found for the given station and date.");

            return Ok(new { Success = success, Message = "Slots de-initialized for the date." });
        }

        // Delete a single slot (Backoffice only)
        [Authorize(Roles = "Backoffice")]
        [HttpDelete("{slotId}")]
        public async Task<IActionResult> DeleteSlot(string slotId)
        {
            var success = await _slotService.DeleteSlotAsync(slotId);
            if (!success)
                return NotFound("Slot not found or already deleted.");

            return Ok(new { Success = success, Message = "Slot deleted successfully." });
        }

    }
}
