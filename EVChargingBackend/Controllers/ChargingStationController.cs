/****************************************************
 * File Name:ChargingStationController.cs
 * Description: Defining Endpoint and Role authentication for ChargingStations .
 * Author: Avindi Obeyesekere
 * Last Changes Date: 2025-10-09
 ****************************************************/
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using EVChargingBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationController : ControllerBase
    {
        private readonly IChargingStationService _stationService;

        public ChargingStationController(IChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateStation([FromBody] ChargingStation station)
        {
            if (string.IsNullOrEmpty(station.Name) || string.IsNullOrEmpty(station.Address) || string.IsNullOrEmpty(station.Type))
                return BadRequest("Name, Address, and Type are required.");

            // Optional: validate geo coordinates when provided
            if (station.GeoLocation != null)
            {
                if (station.GeoLocation.Latitude.HasValue && (station.GeoLocation.Latitude < -90 || station.GeoLocation.Latitude > 90))
                    return BadRequest("Latitude must be between -90 and 90.");
                if (station.GeoLocation.Longitude.HasValue && (station.GeoLocation.Longitude < -180 || station.GeoLocation.Longitude > 180))
                    return BadRequest("Longitude must be between -180 and 180.");
            }

            var createdStation = await _stationService.CreateStationAsync(station);
            return Ok(createdStation);
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPut("update/{stationId}")]
        public async Task<IActionResult> Update(string stationId, [FromBody] ChargingStation station)
        {
            var updated = await _stationService.UpdateStationAsync(stationId, station);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPatch("partial/{stationId}")]
        public async Task<IActionResult> PartialUpdate(string stationId, [FromBody] ChargingStationUpdateDto updatedFields)
        {
            var updated = await _stationService.UpdateStationPartialAsync(stationId, updatedFields);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPost("deactivate/{stationId}")]
        public async Task<IActionResult> Deactivate(string stationId)
        {
            var success = await _stationService.DeactivateStationAsync(stationId);
            if (!success) return BadRequest("Cannot deactivate station with active bookings.");
            return Ok(new { Success = true });
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPost("activate/{stationId}")]
        public async Task<IActionResult> Activate(string stationId)
        {
            var success = await _stationService.ActivateStationAsync(stationId);
            if (!success) return NotFound();
            return Ok(new { Success = true });
        }

        [Authorize(Roles = "Backoffice")]
        [HttpDelete("delete/{stationId}")]
        public async Task<IActionResult> Delete(string stationId)
        {
            var success = await _stationService.DeleteStationAsync(stationId);
            if (!success) return NotFound();
            return Ok(new { Success = true });
        }

        [Authorize(Roles = "Backoffice,EVOwner,StationOperator")]
        [HttpGet("{stationId}")]
        public async Task<IActionResult> GetById(string stationId)
        {
            var station = await _stationService.GetStationByIdAsync(stationId);
            if (station == null) return NotFound();
            
            // Convert to DTO with proper type handling
            var stationDto = new ChargingStationResponseDto
            {
                Id = station.Id,
                Name = station.Name,
                Location = station.Location,
                GeoLocation = station.GeoLocation,
                Type = station.Type,
                Active = ConvertToBoolean(station.Active)
            };
            
            return Ok(stationDto);
        }

        [Authorize(Roles = "Backoffice,EVOwner,StationOperator")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            List<ChargingStation> stations = await _stationService.GetAllStationsAsync();
            
            // Convert to DTOs with proper type handling
            var stationDtos = stations.Select(s => new ChargingStationResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                GeoLocation = s.GeoLocation,
                Type = s.Type,
                Active = ConvertToBoolean(s.Active)
            }).ToList();
            
            return Ok(stationDtos);
        }
        
        private bool ConvertToBoolean(object? value)
        {
            if (value == null) return true;
            
            if (value is bool boolValue)
                return boolValue;
            
            if (value is string stringValue)
            {
                return stringValue.ToLower() == "true" || stringValue == "1";
            }
            
            return true; // Default to true
        }
    }
}
