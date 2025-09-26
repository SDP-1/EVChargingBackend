namespace EVChargingBackend.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }        // still the primary key

        public string Username { get; set; }    // used for login
        public string PasswordHash { get; set; }
        public string Role { get; set; }        // "Backoffice", "StationOperator", "EVOwner"

        [BsonIgnoreIfNull]
        public string? NIC { get; set; }         // Required if Role == "EVOwner"

        public bool Active { get; set; } = false; // EVOwner can deactivate; Backoffice can reactivate
    }
}
