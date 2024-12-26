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
        public string? Email { get; set; }
        public string? MobileDeviceToken { get; set; }
        public List<AlertPreference> AlertPreferences { get; set; } = new();
    }
}
