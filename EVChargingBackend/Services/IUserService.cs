using EVChargingBackend.DTOs;
using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User?> SetUserActiveStatusAsync(string userId, bool active);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<long> GetPendingUserApprovalCountAsync(string role = "EVOwner");
        Task<List<User>> GetAllUsersAsync();
        Task<long> GetUserCountAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<bool> UpdateUserDetailsAsync(string userId, UserUpdateDto updateDto);
    }
}
