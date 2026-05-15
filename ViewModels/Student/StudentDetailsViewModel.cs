namespace WebApplication1.ViewModels.Student
{
    public class StudentDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string DivisionName { get; set; } = string.Empty;
        public int SubjectCount { get; set; }
        public string Subject1 { get; set; } = string.Empty;
        public string? Subject2 { get; set; }
        public string? Subject3 { get; set; }
        public string? Subject4 { get; set; }
        public string? Subject5 { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}