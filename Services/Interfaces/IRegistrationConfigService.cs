using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface IRegistrationConfigService
    {
        Task<RegistrationConfig> GetConfigAsync();
        Task UpdateConfigAsync(RegistrationConfig config);
        Task<bool> IsRegistrationOpenAsync();
    }
}
