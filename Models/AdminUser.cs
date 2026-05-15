using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;
        // ← هيتخزن بعد تشفير BCrypt
    }
}