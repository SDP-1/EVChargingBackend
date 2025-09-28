using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EVChargingBackend.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }          // MongoDB ObjectId of EVOwner
        public string StationId { get; set; }       // Charging station ID
        public DateTime ReservationDateTime { get; set; }
        public bool Approved { get; set; } = true;      // Approved by backoffice
        public bool Confirmed { get; set; }         // Confirmed by station operator
        public bool Completed { get; set; }         // Operation finalized
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Canceled { get; set; }
    }
}
