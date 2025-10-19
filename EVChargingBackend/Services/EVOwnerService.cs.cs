/****************************************************
 * File Name: EVOWnerService.cs
 * Description: EVOWnerService for EVOwners.
 * Author: Avindi Obeyesekere
 * Date: 2025-09-26
 ****************************************************/
using EVChargingBackend.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EVChargingBackend.Services
{
    public class EVOwnerService : IEVOwnerService
    {
        private readonly IMongoCollection<User> _users;

        public EVOwnerService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> GetByNICAsync(string nic)
        {
            return await _users.Find(u => u.Role == "EVOwner" && u.NIC == nic).FirstOrDefaultAsync();
        }

        public async Task<User> CreateEVOwnerAsync(User evoOwner)
        {
            if (evoOwner.Role != "EVOwner")
                throw new InvalidOperationException("User role must be EVOwner.");
            if (string.IsNullOrEmpty(evoOwner.NIC))
                throw new InvalidOperationException("NIC is required for EVOwner.");

            await _users.InsertOneAsync(evoOwner);
            return evoOwner;
        }

        public async Task<User> UpdateEVOwnerAsync(string nic, User updatedEVOwner)
        {
            var filter = Builders<User>.Filter.Eq(u => u.NIC, nic) & Builders<User>.Filter.Eq(u => u.Role, "EVOwner");
            var update = Builders<User>.Update
                .Set(u => u.Username, updatedEVOwner.Username)
                .Set(u => u.PasswordHash, updatedEVOwner.PasswordHash)
                .Set(u => u.Active, updatedEVOwner.Active);

            await _users.UpdateOneAsync(filter, update);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteEVOwnerAsync(string nic)
        {
            var result = await _users.DeleteOneAsync(u => u.NIC == nic && u.Role == "EVOwner");
            return result.DeletedCount > 0;
        }

        public async Task<bool> ActivateEVOwnerAsync(string nic)
        {
            var filter = Builders<User>.Filter.Eq(u => u.NIC, nic) & Builders<User>.Filter.Eq(u => u.Role, "EVOwner");
            var update = Builders<User>.Update.Set(u => u.Active, true);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeactivateEVOwnerAsync(string nic)
        {
            var filter = Builders<User>.Filter.Eq(u => u.NIC, nic) & Builders<User>.Filter.Eq(u => u.Role, "EVOwner");
            var update = Builders<User>.Update.Set(u => u.Active, false);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        //get all evowners active & inactive
        public async Task<List<User>> GetAllEVOwnersAsync()
        {
            return await _users.Find(u => u.Role == "EVOwner").ToListAsync();
        }
        //get all active ev owners
        public async Task<List<User>> GetActiveBackofficeEVOwnersAsync()
        {
            return await _users.Find(u => u.Role == "EVOwner" && u.Active == true).ToListAsync();
        }
        //get evwoners by mapping nic to userid, to save evowners userid in booking when backoffice create booking
        public async Task<string> GetUserIdByNICAsync(string nic)
        {
            var evOwner = await _users.Find(u => u.Role == "EVOwner" && u.NIC == nic).FirstOrDefaultAsync();
            if (evOwner == null)
                return null;  // or throw an exception if you prefer
            return evOwner.Id;  // Return the userId (which is stored as Id in MongoDB)
        }

    }
}
