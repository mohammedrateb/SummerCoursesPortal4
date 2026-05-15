using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Student
{
    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [MaxLength(200)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

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