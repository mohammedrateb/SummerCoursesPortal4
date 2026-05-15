using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels.Admin
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "العنوان مطلوب")]
        [MaxLength(300)]
        [Display(Name = "العنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "المحتوى مطلوب")]
        [Display(Name = "المحتوى")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "ملفات مرفقة")]
        public List<IFormFile>? Files { get; set; }

        [Display(Name = "روابط تضمين")]
        public string? EmbedUrls { get; set; }
    }
}