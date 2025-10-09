using System.Collections.Generic;
using System.Threading.Tasks;
using EVChargingBackend.Models;
using EVChargingBackend.DTOs;

namespace EVChargingBackend.Services
{
    public interface IChargingStationService
    {
        Task<ChargingStation> CreateStationAsync(ChargingStation station);
        Task<ChargingStation> UpdateStationAsync(string stationId, ChargingStation updatedStation);
        Task<bool> DeactivateStationAsync(string stationId);  // Check active bookings first
        Task<ChargingStation> GetStationByIdAsync(string stationId);
        Task<List<ChargingStation>> GetAllStationsAsync();
        Task<bool> ActivateStationAsync(string stationId);
        Task<ChargingStation> UpdateStationPartialAsync(string stationId, ChargingStationUpdateDto updatedFields);
        Task<bool> DeleteStationAsync(string stationId);  // Delete station and all related bookings
    }
}
