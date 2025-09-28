using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBackend.Models
{
    public class ChargingStation
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }           // "AC" or "DC"
        public bool Active { get; set; } = true;   // Station status
    }
}
