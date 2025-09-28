using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EVChargingBackend.Models
{
    public class ChargingSlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }   // store as string in C#

        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; }   // Link to ChargingStation

        public DateTime StartTime { get; set; }       // Slot start time
        public DateTime EndTime { get; set; }         // Slot end time
        public bool IsBooked { get; set; } = false;   // Whether slot is booked

        [BsonRepresentation(BsonType.ObjectId)]
        public string? BookingId { get; set; }        // Optional: link to Booking

        [BsonRepresentation(BsonType.ObjectId)]
        public string? EVOwnerId { get; set; }        // Optional: the EV owner who booked
    }
}
