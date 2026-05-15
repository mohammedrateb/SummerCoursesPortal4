using WebApplication1.Models;

namespace WebApplication1.ViewModels.Admin
{
    public class AdminStudentsViewModel
    {
        public IEnumerable<WebApplication1.Models.Student> Students { get; set; }
            = new List<WebApplication1.Models.Student>();

        public IEnumerable<WebApplication1.Models.Level> Levels { get; set; }
            = new List<WebApplication1.Models.Level>();

        public IEnumerable<WebApplication1.Models.Division> Divisions { get; set; }
            = new List<WebApplication1.Models.Division>();

        public int? SelectedLevelId { get; set; }
        public int? SelectedDivisionId { get; set; }
        public string? SearchTerm { get; set; }
        public string? PaymentFilter { get; set; }

        // ───── Totals (from full filtered set, not current page) ─────
        public int TotalFilteredCount { get; set; }
        public int TotalPaidCount { get; set; }
        public int TotalUnpaidCount { get; set; }

        public int TotalCount => TotalFilteredCount;
        public int PaidCount => TotalPaidCount;
        public int UnpaidCount => TotalUnpaidCount;

        // ───── Pagination ─────
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalPages => (int)Math.Ceiling((double)TotalFilteredCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
