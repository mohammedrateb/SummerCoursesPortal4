using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [MaxLength(200)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [StringLength(14, MinimumLength = 14,
         ErrorMessage = "الرقم القومي يجب أن يكون 14 رقم")]
        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الموبايل مطلوب")]
        [StringLength(11, MinimumLength = 11,
         ErrorMessage = "رقم الموبايل يجب أن يكون 11 رقم")]
        [Display(Name = "رقم الموبايل")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "عدد المواد مطلوب")]
        [Range(1, 5, ErrorMessage = "عدد المواد من 1 إلى 5")]
        [Display(Name = "عدد المواد")]
        public int SubjectCount { get; set; }

        [Required(ErrorMessage = "المادة الأولى مطلوبة")]
        [MaxLength(200)]
        [Display(Name = "المادة الأولى")]
        public string Subject1 { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "المادة الثانية")]
        public string? Subject2 { get; set; }

        [MaxLength(200)]
        [Display(Name = "المادة الثالثة")]
        public string? Subject3 { get; set; }

        [MaxLength(200)]
        [Display(Name = "المادة الرابعة")]
        public string? Subject4 { get; set; }

        [MaxLength(200)]
        [Display(Name = "المادة الخامسة")]
        public string? Subject5 { get; set; }

        [Display(Name = "حالة الدفع")]
        public bool PaymentStatus { get; set; } = false;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(16)]
        [Display(Name = "كود الوصول")]
        public string? AccessCode { get; set; }

        // ───── Soft Delete ─────
        [Display(Name = "محذوف")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "تاريخ الحذف")]
        public DateTime? DeletedAt { get; set; }

        // ───── Foreign Keys ─────

        [Required(ErrorMessage = "المستوى مطلوب")]
        [Display(Name = "المستوى")]
        public int LevelId { get; set; }

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        [Display(Name = "الشعبة / القسم")]
        public int DivisionId { get; set; }

        // ───── Navigation Properties ─────

        [ForeignKey("LevelId")]
        public Level? Level { get; set; }

        [ForeignKey("DivisionId")]
        public Division? Division { get; set; }
    }
}
