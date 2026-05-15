using Microsoft.AspNetCore.Http;
using WebApplication1.Models;
using WebApplication1.ViewModels.Admin;

namespace WebApplication1.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<Post?> GetPostByIdAsync(int id);
        Task<Post> CreatePostAsync(CreatePostViewModel model);
        Task UpdatePostAsync(EditPostViewModel model);
        Task DeletePostAsync(int id);
        Task DeleteAllPostsAsync();
    }
}