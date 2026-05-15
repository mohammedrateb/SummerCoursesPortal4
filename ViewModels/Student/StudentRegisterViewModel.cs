using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Student
{
    public class StudentRegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [MaxLength(200, ErrorMessage = "الاسم طويل جداً")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يكون 14 رقم")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يحتوي على أرقام فقط")]
        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الموبايل مطلوب")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الموبايل يجب أن يكون 11 رقم")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الموبايل يجب أن يحتوي على أرقام فقط")]
        [Display(Name = "رقم الموبايل")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "المستوى مطلوب")]
        [Display(Name = "المستوى")]
        public int LevelId { get; set; }

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        [Display(Name = "الشعبة")]
        public int DivisionId { get; set; }

        [Range(1, 5, ErrorMessage = "عدد المواد يجب أن يكون من 1 إلى 5")]
        [Display(Name = "عدد المواد")]
        public int SubjectCount { get; set; } = 1;

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

        // للـ Dropdowns
        public IEnumerable<WebApplication1.Models.Level> Levels { get; set; } = new List<WebApplication1.Models.Level>();
        public IEnumerable<WebApplication1.Models.Division> Divisions { get; set; } = new List<WebApplication1.Models.Division>();
    }
}