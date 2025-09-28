using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string UserId { get; set; }          // MongoDB ObjectId of EVOwner or BackOffice
    public string StationId { get; set; }       // Station Mongo ObjectId
    public string SlotId { get; set; }          // Optional: link to ChargingSlot
    public DateTime ReservationDateTime { get; set; }  // Assigned automatically from Slot.StartTime
    public bool Approved { get; set; } = true;
    public bool Confirmed { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Canceled { get; set; }
}
