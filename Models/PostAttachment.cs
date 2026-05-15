using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public enum AttachmentType
    {
        Image = 1,
        Video = 2,
        PDF = 3,
        File = 4,
        Embed = 5
    }

    public class PostAttachment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        public AttachmentType Type { get; set; }

        // للـ Embed من موقع تاني
        [MaxLength(2000)]
        public string? EmbedUrl { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation
        public Post? Post { get; set; }
    }
}