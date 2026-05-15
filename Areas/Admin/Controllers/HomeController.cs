using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Admin;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IPostService _postService;
        private readonly IRegistrationConfigService _regConfigService;

        public HomeController(
            IStudentService studentService,
            IPostService postService,
            IRegistrationConfigService regConfigService)
        {
            _studentService = studentService;
            _postService = postService;
            _regConfigService = regConfigService;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername");

            var students = (await _studentService.GetAllStudentsAsync()).ToList();
            var posts = (await _postService.GetAllPostsAsync()).ToList();
            var regConfig = await _regConfigService.GetConfigAsync();

            var now = DateTime.UtcNow;
            var last7 = Enumerable.Range(0, 7)
                .Select(i => now.AddDays(-6 + i).Date)
                .ToList();

            var regLast7 = last7.ToDictionary(
                d => d.ToString("MM/dd"),
                d => students.Count(s => s.CreatedAt.Date == d));

            var studentsByLevel = students
                .Where(s => s.Level != null)
                .GroupBy(s => s.Level!.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            var studentsByDivision = students
                .Where(s => s.Division != null)
                .GroupBy(s => s.Division!.Name.Trim())
                .OrderByDescending(g => g.Count())
                .Take(8)
                .ToDictionary(g => g.Key, g => g.Count());

            var vm = new AdminDashboardViewModel
            {
                TotalStudents = students.Count,
                PaidStudents = students.Count(s => s.PaymentStatus),
                UnpaidStudents = students.Count(s => !s.PaymentStatus),
                TotalPosts = posts.Count,
                StudentsByLevel = studentsByLevel,
                StudentsByDivision = studentsByDivision,
                RegistrationsLast7Days = regLast7.Select(kv => (kv.Key, kv.Value)).ToList(),
                RegistrationIsOpen = regConfig.IsRegistrationOpen()
            };

            return View(vm);
        }
    }
}
