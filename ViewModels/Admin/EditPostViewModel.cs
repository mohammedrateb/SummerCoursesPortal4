using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Admin
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب")]
        [MaxLength(300)]
        [Display(Name = "العنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "المحتوى مطلوب")]
        [Display(Name = "المحتوى")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "ملفات جديدة")]
        public List<IFormFile>? NewFiles { get; set; }

        [Display(Name = "روابط تضمين جديدة")]
        public string? NewEmbedUrls { get; set; }

        // المرفقات الحالية
        public List<PostAttachment> ExistingAttachments { get; set; }
            = new List<PostAttachment>();

        // أرقام المرفقات اللى المستخدم اختار يحذفها فى وقت الحفظ
        [Display(Name = "حذف المرفقات")]
        public List<int> RemoveAttachmentIds { get; set; }
            = new List<int>();
    }
}