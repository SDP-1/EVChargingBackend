using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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
        
        [BsonIgnoreIfNull]
        public object? GeoLocation { get; set; }   // Geographic coordinates as Document - ignore if null
        
        [BsonIgnoreIfNull]
        public int? NumberOfConnectors { get; set; }  // Number of connectors - ignore if null
        
        [BsonIgnoreIfNull]
        public object? ConnectorTypes { get; set; }   // Array of connector types - ignore if null
        
        [BsonIgnoreIfNull]
        public object? OperatingHours { get; set; }   // Operating hours as Document - ignore if null
        
        public string Type { get; set; }           // "AC" or "DC"
        
        [BsonIgnoreIfNull]
        public object? Active { get; set; }         // Station status - can be string or boolean
        
        // This will capture any additional fields that are not explicitly mapped
        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }
    }
}
