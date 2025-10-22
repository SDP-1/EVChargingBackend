using EVChargingBackend.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EVChargingBackend.Services
{
    public interface IEVOwnerService
    {
        Task<User> GetByNICAsync(string nic);                  // only EVOwner
        Task<User> CreateEVOwnerAsync(User evoOwner);         // Role == "EVOwner"
        Task<User> UpdateEVOwnerAsync(string nic, User updatedEVOwner);
        Task<bool> DeleteEVOwnerAsync(string nic);
        Task<bool> ActivateEVOwnerAsync(string nic);         // Backoffice only
        Task<bool> DeactivateEVOwnerAsync(string nic);       // EVOwner themselves
        Task<List<User>> GetAllEVOwnersAsync();
        Task<List<User>> GetActiveBackofficeEVOwnersAsync(); //active evowners get all for backoffice
        Task<string> GetUserIdByNICAsync(string nic); //map user id of evonwer from nic

        // Search EVOwners by a NIC fragment (case-insensitive, substring match)
        Task<List<User>> SearchEVOwnersByNICAsync(string nicFragment);
    }
}
