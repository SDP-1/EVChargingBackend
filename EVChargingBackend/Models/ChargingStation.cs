/****************************************************
 * File Name: ChargingStation.cs
 * Description: Model for Stations.
 * Author: Avindi Obeyesekere
 * Date: 2025-09-24
 ****************************************************/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBackend.Models
{
    public class GeoLocation
    {
        // Latitude and Longitude in decimal degrees
        [BsonElement("lat")]
        public double? Latitude { get; set; }

        [BsonElement("lon")]
        public double? Longitude { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ChargingStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }  // store as string in C#

        public string Name { get; set; }

        // Map existing DB field 'Location' to the Address property for backwards compatibility
        [BsonElement("Location")]
        public string Address { get; set; }

        // Geographical coordinates for mapping (optional)
        [BsonElement("GeoLocation")]
        [BsonIgnoreIfNull]
        public GeoLocation? GeoLocation { get; set; }

        // Station meta
        public string Type { get; set; }           // "AC" or "DC"
        public bool Active { get; set; } = true;   // Station status

        // Additional details
        public int NumberOfConnectors { get; set; } = 1;

        [BsonIgnoreIfNull]
        public List<string>? ConnectorTypes { get; set; } // e.g. ["Type2","CHAdeMO"]

        public string? OperatingHours { get; set; } // human readable

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public bool IsPublic { get; set; } = true;

        [BsonIgnoreIfNull]
        public List<string>? Amenities { get; set; } // e.g. ["Restroom","Cafe","Parking"]

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
