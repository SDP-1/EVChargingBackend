using MongoDB.Bson;
using MongoDB.Driver;
using EVChargingBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVChargingBackend.Services
{
    public interface IChargingSlotService
    {
        Task InitializeDailySlotsAsync(ObjectId stationId, DateTime date);
        Task<List<ChargingSlot>> GetAvailableSlotsAsync(ObjectId stationId, DateTime date);
        Task<bool> BookSlotAsync(ObjectId slotId, string evoOwnerId, string bookingId);
    }

    public class ChargingSlotService : IChargingSlotService
    {
        private readonly IMongoCollection<ChargingSlot> _slots;

        public ChargingSlotService(IMongoDatabase database)
        {
            _slots = database.GetCollection<ChargingSlot>("ChargingSlots");
        }

        // Initialize fixed 1-hour slots for a station on a date
        public async Task InitializeDailySlotsAsync(ObjectId stationId, DateTime date)
        {
            var slots = new List<ChargingSlot>();
            var startHour = 8;
            var endHour = 18;

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
        public async Task<List<ChargingSlot>> GetAvailableSlotsAsync(ObjectId stationId, DateTime date)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq(s => s.StationId, stationId) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, false) &
                         Builders<ChargingSlot>.Filter.Gte(s => s.StartTime, date.Date) &
                         Builders<ChargingSlot>.Filter.Lt(s => s.StartTime, date.Date.AddDays(1));

            return await _slots.Find(filter).ToListAsync();
        }

        // Book a slot
        public async Task<bool> BookSlotAsync(ObjectId slotId, string evoOwnerId, string bookingId)
        {
            var filter = Builders<ChargingSlot>.Filter.Eq(s => s.Id, slotId) &
                         Builders<ChargingSlot>.Filter.Eq(s => s.IsBooked, false);

            var update = Builders<ChargingSlot>.Update
                .Set(s => s.IsBooked, true)
                .Set(s => s.EVOwnerId, evoOwnerId)
                .Set(s => s.BookingId, bookingId);

            var result = await _slots.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
