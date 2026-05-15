namespace WebApplication1.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int PaidStudents { get; set; }
        public int UnpaidStudents { get; set; }
        public int TotalPosts { get; set; }

        public Dictionary<string, int> StudentsByLevel { get; set; } = new();
        public Dictionary<string, int> StudentsByDivision { get; set; } = new();

        public List<(string Date, int Count)> RegistrationsLast7Days { get; set; } = new();

        public bool RegistrationIsOpen { get; set; }

        public double PaidPercentage =>
            TotalStudents > 0 ? Math.Round(PaidStudents * 100.0 / TotalStudents, 1) : 0;
    }
}
