/****************************************************
 * File Name: User.cs
 * Description: Model for Slots.
 * Author: Avindi Obeyesekere
 * Date: 2025-09-24
 ****************************************************/
namespace EVChargingBackend.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }   // store as string in C#

        public string Username { get; set; }    // used for login
        public string PasswordHash { get; set; }
        public string Role { get; set; }        // "Backoffice", "StationOperator", "EVOwner"

        [BsonIgnoreIfNull]
        public string? NIC { get; set; }         // Required if Role == "EVOwner"

        public bool Active { get; set; } = false; // EVOwner can deactivate; Backoffice can reactivate
        public string Message { get; set; } = "NewUser";
    }
}
