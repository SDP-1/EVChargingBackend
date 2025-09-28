// Add the necessary namespaces for MongoDB and Task
using System.Threading.Tasks;
using EVChargingBackend.Models;  // Assuming the User model is in this namespace
using MongoDB.Bson;
using MongoDB.Driver;

namespace EVChargingBackend.Services
{
    // UserService Implementation
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        // Constructor to initialize MongoDB connection
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        // Method to create a new user
        public async Task<User> CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        // Method to get a user by their username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<bool> SetUserActiveStatusAsync(string userId, bool active)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.Active, active);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _users.Find(u => u.Role == role).ToListAsync();
        }

        // Get count of users with Active=false (pending approval)
        public async Task<long> GetPendingUserApprovalCountAsync(string role = "EVOwner")
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, role) &
                         Builders<User>.Filter.Eq(u => u.Active, false);

            return await _users.CountDocumentsAsync(filter);
        }
    }
}
