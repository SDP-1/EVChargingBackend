// Add the necessary namespaces for MongoDB and Task
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;  // Assuming the User model is in this namespace
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

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

        public async Task<User?> SetUserActiveStatusAsync(string userId, bool active)
        {
            // First get the current user
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return null;

            // Always update Active flag
            var updates = new List<UpdateDefinition<User>>
            {
                Builders<User>.Update.Set(u => u.Active, active)
            };

            if (active)
            {
                // Only check status when activating
                if (string.Equals(user.Message, "NewUser", StringComparison.OrdinalIgnoreCase))
                {
                    updates.Add(
                        Builders<User>.Update.Set(
                            u => u.Message,
                            $"Active since {DateTime.UtcNow:yyyy-MM-dd}"
                        )
                    );
                }
            }

            var update = Builders<User>.Update.Combine(updates);

            await _users.UpdateOneAsync(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                update
            );

            // Return the updated user
            var updatedUser = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return updatedUser;
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

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        // Get total user count
        public async Task<long> GetUserCountAsync()
        {
            return await _users.CountDocumentsAsync(_ => true);
        }

        // Implementation for updating user details
        public async Task<bool> UpdateUserDetailsAsync(string userId, UserUpdateDto updateDto)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var updateDefinitions = new List<UpdateDefinition<User>>();

            // Conditionally add updates based on which DTO fields are provided (not null)
            if (updateDto.NIC != null)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.NIC, updateDto.NIC));
            }
            if (updateDto.Message != null)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Message, updateDto.Message));
            }
            if (updateDto.Active.HasValue) // Use HasValue for nullable bool
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Active, updateDto.Active.Value));
            }

            if (updateDefinitions.Count == 0)
            {
                // Return true if nothing needed to be updated (no error occurred)
                return true;
            }

            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitions);
            var result = await _users.UpdateOneAsync(filter, combinedUpdate);

            // Return true if the user was updated or if they were found but no fields were modified (ModifiedCount = 0)
            return result.IsAcknowledged && (result.ModifiedCount > 0 || result.MatchedCount > 0);
        }
    }
}
