namespace EVChargingBackend.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Username { get; set; }
    public string PasswordHash { get; set; } // Store the password hash
    public string Role { get; set; } // "Backoffice" or "StationOperator"
}

