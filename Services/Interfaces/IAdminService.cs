namespace WebApplication1.Services.Interfaces
{
    public interface IAdminService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    }
}
