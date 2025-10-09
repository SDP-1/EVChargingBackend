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
            if (string.IsNullOrEmpty(station.Name) || string.IsNullOrEmpty(station.Location) || string.IsNullOrEmpty(station.Type))
                return BadRequest("Name, Location, and Type are required.");

            var createdStation = await _stationService.CreateStationAsync(station);
            return Ok(createdStation);
        }

        [Authorize(Roles = "Backoffice")]
        [HttpPut("update/{stationId}")]
        public async Task<IActionResult> Update(string stationId, [FromBody] ChargingStation station)
        {
            var updated = await _stationService.UpdateStationAsync(stationId, station);
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
