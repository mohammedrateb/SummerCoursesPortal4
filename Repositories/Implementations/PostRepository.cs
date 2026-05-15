using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllAsync() =>
            await _context.Posts
                .Include(p => p.Attachments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<Post?> GetByIdAsync(int id) =>
            await _context.Posts
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Post> AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            _context.Posts.RemoveRange(_context.Posts);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> RemoveAttachmentsAsync(
            int postId, IEnumerable<int> attachmentIds)
        {
            var idsSet = attachmentIds?.ToHashSet() ?? new HashSet<int>();
            if (idsSet.Count == 0)
                return Enumerable.Empty<string>();

            var attachments = await _context.PostAttachments
                .Where(a => a.PostId == postId && idsSet.Contains(a.Id))
                .ToListAsync();

            // نرجّع بس مسارات الملفات اللى مش روابط تضمين
            // (روابط التضمين مالهاش ملف على القرص نحذفه).
            var diskPaths = attachments
                .Where(a => a.Type != AttachmentType.Embed
                            && !string.IsNullOrEmpty(a.FilePath))
                .Select(a => a.FilePath!)
                .ToList();

            _context.PostAttachments.RemoveRange(attachments);
            await _context.SaveChangesAsync();

            return diskPaths;
        }
    }
}