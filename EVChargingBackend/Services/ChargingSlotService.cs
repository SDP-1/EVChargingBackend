using MongoDB.Driver;
using EVChargingBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace EVChargingBackend.Services
{
    public interface IChargingSlotService
    {
        Task InitializeDailySlotsAsync(string stationId, DateTime date);
        Task<List<ChargingSlot>> GetAvailableSlotsAsync(string stationId, DateTime date);
        Task<ChargingSlot> GetSlotByIdAsync(string slotId);
        Task<List<ChargingSlot>> GetBookedSlotsAsync(string stationId, DateTime date);
        Task<List<ChargingSlot>> GetAllSlotsAsync(string stationId, DateTime date);
        Task<bool> FreeSlotAsync(string slotId); 
        Task<bool> BookSlotAsync(string slotId, string evoOwnerId, string bookingId);
    }

    public class ChargingSlotService : IChargingSlotService
    {
        private readonly IMongoCollection<ChargingSlot> _slots;

        public ChargingSlotService(IMongoDatabase database)
        {
            _slots = database.GetCollection<ChargingSlot>("ChargingSlots");
        }

        // Initialize fixed 1-hour slots for a station on a date
        public async Task InitializeDailySlotsAsync(string stationId, DateTime date)
        {
            var slots = new List<ChargingSlot>();
            int startHour = 8, endHour = 18;

            for (int h = startHour; h < endHour; h++)
            {
                var start = new DateTime(date.Year, date.Month, date.Day, h, 0, 0);
                var end = start.AddHours(1);

                slots.Add(new ChargingSlot
                {
                    StationId = stationId,
                    StartTime = start,
                    EndTime = end,
                    IsBooked = false
                });
            }

            if (slots.Count > 0)
                await _slots.InsertManyAsync(slots);
        }

        // Get free slots for a station on a given day
        public async Task<List<ChargingSlot>> GetAvailableSlotsAsync(string stationId, DateTime date)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq(s => s.StationId, stationId) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, false) &
                         Builders<ChargingSlot>.Filter.Gte(s => s.StartTime, date.Date) &
                         Builders<ChargingSlot>.Filter.Lt(s => s.StartTime, date.Date.AddDays(1));

            return await _slots.Find(filter).ToListAsync();
        }

        // Retrieve a slot by its Id
        public async Task<ChargingSlot> GetSlotByIdAsync(string slotId)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq("_id", ObjectId.Parse(slotId));
            return await _slots.Find(filter).FirstOrDefaultAsync();
        }

        // Get all slots for a station on a date (both booked and available)
        public async Task<List<ChargingSlot>> GetAllSlotsAsync(string stationId, DateTime date)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq(s => s.StationId, stationId) &
                         Builders<ChargingSlot>.Filter.Gte(s => s.StartTime, date.Date) &
                         Builders<ChargingSlot>.Filter.Lt(s => s.StartTime, date.Date.AddDays(1));

            return await _slots.Find(filter).ToListAsync();
        }

        // Get only booked slots
        public async Task<List<ChargingSlot>> GetBookedSlotsAsync(string stationId, DateTime date)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq(s => s.StationId, stationId) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, true) &
                         Builders<ChargingSlot>.Filter.Gte(s => s.StartTime, date.Date) &
                         Builders<ChargingSlot>.Filter.Lt(s => s.StartTime, date.Date.AddDays(1));

            return await _slots.Find(filter).ToListAsync();
        }

        // Free a booked slot (mark IsBooked = false, clear BookingId and EVOwnerId)
        public async Task<bool> FreeSlotAsync(string slotId)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq("_id", ObjectId.Parse(slotId)) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, true);

            var update = Builders<ChargingSlot>.Update
                .Set(s => s.IsBooked, false)
                .Set(s => s.BookingId, null)
                .Set(s => s.EVOwnerId, null);

            var result = await _slots.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Book a slot (mark IsBooked = true, link to Booking and EVOwner)
        public async Task<bool> BookSlotAsync(string slotId, string evoOwnerId, string bookingId)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq("_id", ObjectId.Parse(slotId)) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, false); // ensure slot not already booked

            var update = Builders<ChargingSlot>.Update
                .Set(s => s.IsBooked, true)
                .Set(s => s.EVOwnerId, evoOwnerId)
                .Set(s => s.BookingId, bookingId);

            var result = await _slots.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

    }
}
