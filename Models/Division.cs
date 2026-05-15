using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Division
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "الشعبة / القسم")]
        public string Name { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Student> Students { get; set; }
                                    = new List<Student>();
    }
}