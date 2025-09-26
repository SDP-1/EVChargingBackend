using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBackend.Models
{
    public class Booking
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string EVOwnerNIC { get; set; }      // EV Owner identifier
        public string StationId { get; set; }       // Charging station ID
        public DateTime ReservationDateTime { get; set; }
        public bool Approved { get; set; }          // Approved by backoffice
        public bool Confirmed { get; set; }         // Confirmed by station operator
        public bool Completed { get; set; }         // Operation finalized
        public DateTime CreatedAt { get; set; }     // Optional: when reservation was made
        public DateTime? UpdatedAt { get; set; }   // Optional: last update
        public bool Canceled { get; set; }          // Optional: cancellation flag
    }

}
