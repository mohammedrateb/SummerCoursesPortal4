using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepo;

        public AdminService(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<bool> LoginAsync(string username, string password) =>
            await _adminRepo.ValidateCredentialsAsync(username, password);

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword) =>
            await _adminRepo.ChangePasswordAsync(username, currentPassword, newPassword);
    }
}
