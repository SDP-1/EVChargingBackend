using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public class ChargingStationService : IChargingStationService
    {
        private readonly IMongoCollection<ChargingStation> _stations;
        private readonly IMongoCollection<Booking> _bookings; // To check active bookings

        public ChargingStationService(IMongoDatabase database)
        {
            _stations = database.GetCollection<ChargingStation>("ChargingStations");
            _bookings = database.GetCollection<Booking>("Bookings");
        }

        public async Task<ChargingStation> CreateStationAsync(ChargingStation station)
        {
            await _stations.InsertOneAsync(station);
            return station;
        }

        public async Task<ChargingStation> UpdateStationAsync(string stationId, ChargingStation updatedStation)
        {
            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, ObjectId.Parse(stationId));
            var update = Builders<ChargingStation>.Update
                .Set(s => s.Name, updatedStation.Name)
                .Set(s => s.Location, updatedStation.Location)
                .Set(s => s.Type, updatedStation.Type)
                .Set(s => s.AvailableSlots, updatedStation.AvailableSlots);

            await _stations.UpdateOneAsync(filter, update);
            return await _stations.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeactivateStationAsync(string stationId)
        {
            // Check if any active bookings exist for this station
            var activeBookingFilter = Builders<Booking>.Filter.Eq(b => b.StationId, stationId) &
                                      Builders<Booking>.Filter.Eq(b => b.Canceled, false) &
                                      Builders<Booking>.Filter.Eq(b => b.Completed, false);

            var activeBookings = await _bookings.Find(activeBookingFilter).AnyAsync();
            if (activeBookings) return false;  // Cannot deactivate

            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, ObjectId.Parse(stationId));
            var update = Builders<ChargingStation>.Update.Set(s => s.Active, false);
            var result = await _stations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<ChargingStation> GetStationByIdAsync(string stationId)
        {
            return await _stations.Find(s => s.Id == ObjectId.Parse(stationId)).FirstOrDefaultAsync();
        }

        public async Task<List<ChargingStation>> GetAllStationsAsync()
        {
            return await _stations.Find(_ => true).ToListAsync();
        }
    }
}
