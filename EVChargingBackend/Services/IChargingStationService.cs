using System.Collections.Generic;
using System.Threading.Tasks;
using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public interface IChargingStationService
    {
        Task<ChargingStation> CreateStationAsync(ChargingStation station);
        Task<ChargingStation> UpdateStationAsync(string stationId, ChargingStation updatedStation);
        Task<bool> DeactivateStationAsync(string stationId);  // Check active bookings first
        Task<ChargingStation> GetStationByIdAsync(string stationId);
        Task<List<ChargingStation>> GetAllStationsAsync();
    }
}
