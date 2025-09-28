using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBackend.Models
{
    public class ChargingStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }  // store as string in C#

        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }           // "AC" or "DC"
        public bool Active { get; set; } = true;   // Station status
    }
}
