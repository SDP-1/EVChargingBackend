using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> SetUserActiveStatusAsync(string userId, bool active);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<long> GetPendingUserApprovalCountAsync(string role = "EVOwner");
    }
}
