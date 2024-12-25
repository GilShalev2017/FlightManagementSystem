using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FlightManagementSystem.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Maps to MongoDB's `_id`
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? MobileDeviceToken { get; set; }
        public List<UserAlertPreference> AlertPreferences { get; set; } = new();
    }
}
