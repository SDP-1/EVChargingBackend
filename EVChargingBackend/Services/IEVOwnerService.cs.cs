using EVChargingBackend.Models;
using System.Threading.Tasks;

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
    }
}
