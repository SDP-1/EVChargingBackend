using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EVChargingBackend.Models
{
    public class ChargingSlot
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId StationId { get; set; }       // Link to ChargingStation
        public DateTime StartTime { get; set; }       // Slot start time
        public DateTime EndTime { get; set; }         // Slot end time
        public bool IsBooked { get; set; } = false;   // Whether slot is booked
        public string? BookingId { get; set; }        // Optional: link to Booking
        public string? EVOwnerId { get; set; }        // Optional: the EV owner who booked
    }
}
