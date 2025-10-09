using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVChargingBackend.Models;
using EVChargingBackend.DTOs;
using System;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace EVChargingBackend.Services
{
    public class ChargingStationService : IChargingStationService
    {
        private readonly IMongoCollection<ChargingStation> _stations;
        private readonly IMongoCollection<Booking> _bookings; // To check active bookings
        private readonly IMongoCollection<ChargingSlot> _slots; // To delete slots when deleting station
        private readonly IMapper _mapper;

        public ChargingStationService(IMongoDatabase database, IMapper mapper)
        {
            _stations = database.GetCollection<ChargingStation>("ChargingStations");
            _bookings = database.GetCollection<Booking>("Bookings");
            _slots = database.GetCollection<ChargingSlot>("ChargingSlots");
            _mapper = mapper;
        }

        public async Task<ChargingStation> CreateStationAsync(ChargingStation station)
        {
            station.CreatedAt = DateTime.Now;
            await _stations.InsertOneAsync(station);
            return station;
        }

        public async Task<ChargingStation> UpdateStationAsync(string stationId, ChargingStation updatedStation)
        {
            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, stationId);
            var update = Builders<ChargingStation>.Update
                .Set(s => s.Name, updatedStation.Name)
                .Set(s => s.Address, updatedStation.Address)
                .Set(s => s.GeoLocation, updatedStation.GeoLocation)
                .Set(s => s.Type, updatedStation.Type)
                .Set(s => s.Active, updatedStation.Active)
                .Set(s => s.NumberOfConnectors, updatedStation.NumberOfConnectors)
                .Set(s => s.ConnectorTypes, updatedStation.ConnectorTypes)
                .Set(s => s.OperatingHours, updatedStation.OperatingHours)
                .Set(s => s.PhoneNumber, updatedStation.PhoneNumber)
                .Set(s => s.Email, updatedStation.Email)
                .Set(s => s.IsPublic, updatedStation.IsPublic)
                .Set(s => s.Amenities, updatedStation.Amenities)
                .Set(s => s.UpdatedAt, DateTime.Now);

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

            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, stationId);
            var update = Builders<ChargingStation>.Update.Set(s => s.Active, false).Set(s => s.UpdatedAt, DateTime.Now);
            var result = await _stations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateStationAsync(string stationId)
        {
            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, stationId);
            var update = Builders<ChargingStation>.Update.Set(s => s.Active, true).Set(s => s.UpdatedAt, DateTime.Now);
            var result = await _stations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<ChargingStation> UpdateStationPartialAsync(string stationId, ChargingStationUpdateDto updatedFields)
        {
            var filter = Builders<ChargingStation>.Filter.Eq(s => s.Id, stationId);

            // Map DTO to ChargingStation instance (AutoMapper configured to ignore nulls)
            var mapped = _mapper.Map<ChargingStation>(updatedFields);

            // Convert mapped instance to BsonDocument and remove empty/null fields
            var doc = mapped.ToBsonDocument();

            // Remove fields we should not update or that are null/empty
            doc.Remove("_id");
            doc.Remove("CreatedAt");

            var updates = new List<UpdateDefinition<ChargingStation>>();

            foreach (var elem in doc.Elements)
            {
                // Skip null BsonValues
                if (elem.Value == BsonNull.Value) continue;

                object dotNetValue;

                // If the element is a nested document (e.g. GeoLocation), deserialize to the CLR type explicitly
                if (elem.Value.IsBsonDocument)
                {
                    var bd = elem.Value.AsBsonDocument;

                    // Handle GeoLocation explicitly to avoid wrong shape errors during deserialization
                    if (elem.Name == "GeoLocation")
                    {
                        dotNetValue = BsonSerializer.Deserialize<GeoLocation>(bd);
                    }
                    else
                    {
                        dotNetValue = BsonTypeMapper.MapToDotNetValue(bd);
                    }
                }
                else
                {
                    // Map other BSON values to CLR objects
                    dotNetValue = BsonTypeMapper.MapToDotNetValue(elem.Value);
                }

                // Add update for this field using string field name
                updates.Add(Builders<ChargingStation>.Update.Set(elem.Name, dotNetValue));
            }

            if (updates.Count == 0)
                return await _stations.Find(filter).FirstOrDefaultAsync();

            // Always update UpdatedAt
            updates.Add(Builders<ChargingStation>.Update.Set(s => s.UpdatedAt, DateTime.Now));

            var combined = Builders<ChargingStation>.Update.Combine(updates);
            await _stations.UpdateOneAsync(filter, combined);
            return await _stations.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ChargingStation> GetStationByIdAsync(string stationId)
        {
            return await _stations.Find(s => s.Id == stationId).FirstOrDefaultAsync();
        }

        public async Task<List<ChargingStation>> GetAllStationsAsync()
        {
            return await _stations.Find(_ => true).ToListAsync();
        }

        public async Task<bool> DeleteStationAsync(string stationId)
        {
            // Delete related bookings first
            var bookingFilter = Builders<Booking>.Filter.Eq(b => b.StationId, stationId);
            await _bookings.DeleteManyAsync(bookingFilter);

            // Delete related slots
            var slotFilter = Builders<ChargingSlot>.Filter.Eq(s => s.StationId, stationId);
            await _slots.DeleteManyAsync(slotFilter);

            // Delete the station
            var stationFilter = Builders<ChargingStation>.Filter.Eq(s => s.Id, stationId);
            var result = await _stations.DeleteOneAsync(stationFilter);

            return result.DeletedCount > 0;
        }
    }
}
