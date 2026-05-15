using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task<Post> AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
        Task DeleteAllAsync();

        // يحذف مرفقات بعينها من قاعدة البيانات ويرجّع مساراتها على القرص
        // علشان الـ Service يحذف الملفات الفعلية من wwwroot/uploads.
        Task<IEnumerable<string>> RemoveAttachmentsAsync(
            int postId, IEnumerable<int> attachmentIds);
    }
}