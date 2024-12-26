using FlightManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlightManagementSystem.Infrastructure
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(string id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> UpdateUserAsync(User user);
        Task<User?> DeleteUserAsync(string id);
    }

    public class UserRepository : IUserRepository
    {
        private const string CollectionName = "users";
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("FlightManagementSystem");
            _userCollection = database.GetCollection<User>(CollectionName);
        }

        public async Task<User> AddUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _userCollection.InsertOneAsync(user);
            return user; // Return the inserted user
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var result = await _userCollection.ReplaceOneAsync(filter, user);

            if (result.MatchedCount == 0)
                return null; // User not found

            return user; // Return the updated user
        }

        public async Task<User?> DeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);

            var deletedUser = await _userCollection.FindOneAndDeleteAsync(filter);

            return deletedUser; // Return the deleted user (null if not found)
        }
    }
}