using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class RegistrationConfig
    {
        public int Id { get; set; }

        [Display(Name = "التسجيل مفتوح")]
        public bool IsOpen { get; set; } = true;

        [Display(Name = "تاريخ فتح التسجيل")]
        public DateTime? OpenAt { get; set; }

        [Display(Name = "تاريخ إغلاق التسجيل")]
        public DateTime? CloseAt { get; set; }

        [MaxLength(500)]
        [Display(Name = "رسالة الإغلاق")]
        public string ClosedMessage { get; set; } = "التسجيل مغلق حالياً. يرجى متابعة الإعلانات.";

        public bool IsRegistrationOpen()
        {
            if (!IsOpen) return false;

            var now = DateTime.UtcNow;
            if (OpenAt.HasValue && now < OpenAt.Value) return false;
            if (CloseAt.HasValue && now > CloseAt.Value) return false;

            return true;
        }
    }
}
