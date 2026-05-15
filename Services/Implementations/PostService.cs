using Microsoft.AspNetCore.Http;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Admin;

namespace WebApplication1.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PostService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
            ".mp4", ".webm", ".mov", ".mkv", ".avi",
            ".pdf",
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".zip", ".rar"
        };

        private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js",
            ".jar", ".com", ".scr", ".msi", ".html", ".htm", ".svg", ".php",
            ".aspx", ".asp", ".cshtml", ".config"
        };

        private static readonly HashSet<string> AllowedEmbedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "youtube.com", "www.youtube.com", "youtu.be", "youtube-nocookie.com", "www.youtube-nocookie.com",
            "vimeo.com", "player.vimeo.com",
            "facebook.com", "www.facebook.com", "web.facebook.com", "fb.watch",
            "drive.google.com", "docs.google.com"
        };

        private const long MaxFileSizeBytes = 200L * 1024 * 1024;

        public PostService(
            IPostRepository postRepo,
            IWebHostEnvironment env,
            ILogger<PostService> logger)
        {
            _postRepo = postRepo;
            _env = env;
            _logger = logger;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync() =>
            await _postRepo.GetAllAsync();

        public async Task<Post?> GetPostByIdAsync(int id) =>
            await _postRepo.GetByIdAsync(id);

        public async Task<Post> CreatePostAsync(CreatePostViewModel model)
        {
            var post = new Post
            {
                Title = (model.Title ?? string.Empty).Trim(),
                Content = (model.Content ?? string.Empty).Trim(),
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            await SaveFilesAsync(post, model.Files);
            AddEmbedUrls(post, model.EmbedUrls);

            return await _postRepo.AddAsync(post);
        }

        public async Task UpdatePostAsync(EditPostViewModel model)
        {
            var post = await _postRepo.GetByIdAsync(model.Id);
            if (post == null) return;

            post.Title = (model.Title ?? string.Empty).Trim();
            post.Content = (model.Content ?? string.Empty).Trim();
            post.UpdatedAt = DateTime.UtcNow;

            if (model.RemoveAttachmentIds != null && model.RemoveAttachmentIds.Count > 0)
            {
                var deletedDiskPaths = await _postRepo.RemoveAttachmentsAsync(
                    post.Id, model.RemoveAttachmentIds);

                foreach (var relPath in deletedDiskPaths)
                {
                    try
                    {
                        var safeRel = relPath.TrimStart('/', '\\');
                        var fullPath = Path.Combine(_env.WebRootPath, safeRel);

                        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
                        var fullPathResolved = Path.GetFullPath(fullPath);

                        if (fullPathResolved.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
                            && File.Exists(fullPathResolved))
                        {
                            File.Delete(fullPathResolved);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "فشل حذف الملف من القرص: {Path}", relPath);
                    }
                }

                post.Attachments = post.Attachments
                    .Where(a => !model.RemoveAttachmentIds.Contains(a.Id))
                    .ToList();
            }

            await SaveFilesAsync(post, model.NewFiles);
            AddEmbedUrls(post, model.NewEmbedUrls);

            await _postRepo.UpdateAsync(post);
        }

        public async Task DeletePostAsync(int id) =>
            await _postRepo.DeleteAsync(id);

        public async Task DeleteAllPostsAsync() =>
            await _postRepo.DeleteAllAsync();

        // ════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════
        private async Task SaveFilesAsync(Post post, IList<IFormFile>? files)
        {
            if (files == null || files.Count == 0) return;

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            try
            {
                Directory.CreateDirectory(uploadsPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل إنشاء مجلد المرفقات");
                return;
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                try
                {
                    if (file.Length > MaxFileSizeBytes)
                    {
                        _logger.LogWarning("تم تجاهل ملف أكبر من الحد المسموح: {Name} ({Size})",
                            file.FileName, file.Length);
                        continue;
                    }

                    var rawName = Path.GetFileName(file.FileName ?? string.Empty);
                    var ext = Path.GetExtension(rawName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(ext)
                        || BlockedExtensions.Contains(ext)
                        || !AllowedExtensions.Contains(ext))
                    {
                        _logger.LogWarning("تم رفض امتداد ملف غير مسموح: {Name}", rawName);
                        continue;
                    }

                    var safeStoredName = $"{Guid.NewGuid():N}{ext}";
                    var displayName = SanitizeDisplayName(rawName);
                    var fullPath = Path.Combine(uploadsPath, safeStoredName);

                    var uploadsRoot = Path.GetFullPath(uploadsPath);
                    var fullResolved = Path.GetFullPath(fullPath);
                    if (!fullResolved.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                        continue;

                    await using (var stream = new FileStream(
                        fullPath, FileMode.Create, FileAccess.Write, FileShare.None,
                        bufferSize: 81920, useAsync: true))
                    {
                        await file.CopyToAsync(stream);
                    }

                    post.Attachments.Add(new PostAttachment
                    {
                        FileName = displayName,
                        FilePath = $"/uploads/{safeStoredName}",
                        Type = GetAttachmentType(ext),
                        UploadedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "فشل رفع ملف: {Name}", file?.FileName);
                }
            }
        }

        private void AddEmbedUrls(Post post, string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            var urls = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var url in urls)
            {
                var trimmed = url.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)) continue;
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) continue;

                if (!AllowedEmbedHosts.Contains(uri.Host))
                {
                    _logger.LogWarning("تم رفض رابط Embed من مضيف غير معتمد: {Host}", uri.Host);
                    continue;
                }

                post.Attachments.Add(new PostAttachment
                {
                    FileName = "embed",
                    FilePath = trimmed,
                    EmbedUrl = trimmed,
                    Type = AttachmentType.Embed,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        private static string SanitizeDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "file";
            var invalid = Path.GetInvalidFileNameChars();
            var clean = new string(name.Where(c => !invalid.Contains(c)).ToArray());
            if (clean.Length > 120) clean = clean.Substring(0, 120);
            return string.IsNullOrWhiteSpace(clean) ? "file" : clean;
        }

        private static AttachmentType GetAttachmentType(string ext) => ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp"
                => AttachmentType.Image,
            ".mp4" or ".avi" or ".mov" or ".mkv" or ".webm"
                => AttachmentType.Video,
            ".pdf"
                => AttachmentType.PDF,
            _ => AttachmentType.File
        };
    }
}
