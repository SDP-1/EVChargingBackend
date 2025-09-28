using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> SetUserActiveStatusAsync(string userId, bool active);
    }
}
