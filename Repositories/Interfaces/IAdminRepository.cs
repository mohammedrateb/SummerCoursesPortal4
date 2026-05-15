using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminUser?> GetByUsernameAsync(string username);
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    }
}
