using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Admin;
using WebApplication1.ViewModels.Student;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILevelRepository _levelRepo;
        private readonly IDivisionRepository _divisionRepo;
        private readonly IExcelExportService _excelService;
        private readonly IAdminService _adminService;
        private readonly ILogger<StudentsController> _logger;

        private const int PageSize = 25;

        public StudentsController(
            IStudentService studentService,
            ILevelRepository levelRepo,
            IDivisionRepository divisionRepo,
            IExcelExportService excelService,
            IAdminService adminService,
            ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _levelRepo = levelRepo;
            _divisionRepo = divisionRepo;
            _excelService = excelService;
            _adminService = adminService;
            _logger = logger;
        }

        private bool IsAdminLoggedIn() =>
            HttpContext.Session.GetString("IsAdminLoggedIn") == "true";

        private string AdminUsername() =>
            HttpContext.Session.GetString("AdminUsername") ?? "admin";

        [HttpGet]
        public async Task<IActionResult> Index(
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter,
            int page = 1)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var allStudents = await _studentService.GetAllStudentsAsync();

            if (levelId.HasValue) allStudents = allStudents.Where(s => s.LevelId == levelId.Value);
            if (divisionId.HasValue) allStudents = allStudents.Where(s => s.DivisionId == divisionId.Value);

            if (paymentFilter == "paid") allStudents = allStudents.Where(s => s.PaymentStatus == true);
            else if (paymentFilter == "unpaid") allStudents = allStudents.Where(s => s.PaymentStatus == false);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                allStudents = allStudents.Where(s =>
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.NationalId.Contains(searchTerm));

            var filteredList = allStudents.ToList();
            int totalCount = filteredList.Count;
            int paidCount = filteredList.Count(s => s.PaymentStatus);

            if (page < 1) page = 1;
            var pagedStudents = filteredList
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var model = new AdminStudentsViewModel
            {
                Students = pagedStudents,
                Levels = await _levelRepo.GetAllLevelsAsync(),
                Divisions = await _divisionRepo.GetAllDivisionsAsync(),
                SelectedLevelId = levelId,
                SelectedDivisionId = divisionId,
                SearchTerm = searchTerm,
                PaymentFilter = paymentFilter,
                TotalFilteredCount = totalCount,
                TotalPaidCount = paidCount,
                TotalUnpaidCount = totalCount - paidCount,
                CurrentPage = page,
                PageSize = PageSize
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayment(
            int studentId,
            bool paymentStatus,
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter,
            int page = 1)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            await _studentService.UpdatePaymentStatusAsync(studentId, paymentStatus);
            _logger.LogInformation("[AUDIT] Payment UPDATED: student #{Id} → {Status} by {Admin}",
                studentId, paymentStatus ? "مدفوع" : "غير مدفوع", AdminUsername());

            return RedirectToAction("Index", new { levelId, divisionId, searchTerm, paymentFilter, page });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var model = await _studentService.GetStudentForEditAsync(id, isAdmin: true);
            if (model == null)
            {
                TempData["ErrorMessage"] = "⛔ لا يمكن تعديل بيانات هذا الطالب.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetInt32("AdminEditStudentId", id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentEditViewModel model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var studentId = HttpContext.Session.GetInt32("AdminEditStudentId");
            if (studentId == null)
                return RedirectToAction("Index");

            model.Id = studentId.Value;

            if (!ModelState.IsValid)
            {
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            var result = await _studentService.UpdateStudentAsync(model, isAdmin: true);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            _logger.LogInformation("[AUDIT] Student EDITED: #{Id} by {Admin}", studentId, AdminUsername());
            HttpContext.Session.Remove("AdminEditStudentId");
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int studentId,
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter,
            int page = 1)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var student = await _studentService.GetStudentByIdAsync(studentId);
            var studentName = student?.FullName ?? "؟";

            var result = await _studentService.DeleteStudentAsync(studentId, isAdmin: true);
            _logger.LogInformation("[AUDIT] Student DELETED: #{Id} ({Name}) by {Admin}",
                studentId, studentName, AdminUsername());

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("Index",
                new { levelId, divisionId, searchTerm, paymentFilter, page });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var allStudents = await _studentService.GetAllStudentsAsync();
            if (levelId.HasValue) allStudents = allStudents.Where(s => s.LevelId == levelId.Value);
            if (divisionId.HasValue) allStudents = allStudents.Where(s => s.DivisionId == divisionId.Value);
            if (paymentFilter == "paid") allStudents = allStudents.Where(s => s.PaymentStatus == true);
            else if (paymentFilter == "unpaid") allStudents = allStudents.Where(s => s.PaymentStatus == false);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                allStudents = allStudents.Where(s =>
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.NationalId.Contains(searchTerm));

            _logger.LogInformation("[AUDIT] Excel EXPORTED by {Admin} — {Count} students",
                AdminUsername(), allStudents.Count());

            var fileBytes = _excelService.ExportStudentsToExcel(allStudents);
            var fileName = $"Students_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> Print(
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var allStudents = await _studentService.GetAllStudentsAsync();
            if (levelId.HasValue) allStudents = allStudents.Where(s => s.LevelId == levelId.Value);
            if (divisionId.HasValue) allStudents = allStudents.Where(s => s.DivisionId == divisionId.Value);
            if (paymentFilter == "paid") allStudents = allStudents.Where(s => s.PaymentStatus == true);
            else if (paymentFilter == "unpaid") allStudents = allStudents.Where(s => s.PaymentStatus == false);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                allStudents = allStudents.Where(s =>
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.NationalId.Contains(searchTerm));

            return View(allStudents.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFiltered(
            int? levelId,
            int? divisionId,
            string? searchTerm,
            string? paymentFilter,
            string confirmUsername,
            string confirmPassword)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var isValid = await _adminService.LoginAsync(confirmUsername, confirmPassword);
            if (!isValid)
            {
                TempData["ErrorMessage"] = "❌ اسم المستخدم أو كلمة المرور غير صحيحة";
                return RedirectToAction("Index", new { levelId, divisionId, searchTerm, paymentFilter });
            }

            var allStudents = await _studentService.GetAllStudentsAsync();
            if (levelId.HasValue) allStudents = allStudents.Where(s => s.LevelId == levelId.Value);
            if (divisionId.HasValue) allStudents = allStudents.Where(s => s.DivisionId == divisionId.Value);
            if (paymentFilter == "paid") allStudents = allStudents.Where(s => s.PaymentStatus == true);
            else if (paymentFilter == "unpaid") allStudents = allStudents.Where(s => s.PaymentStatus == false);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                allStudents = allStudents.Where(s =>
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.NationalId.Contains(searchTerm));

            var studentsList = allStudents.ToList();
            int count = studentsList.Count;

            foreach (var student in studentsList)
                await _studentService.DeleteStudentAsync(student.Id, isAdmin: true);

            _logger.LogWarning("[AUDIT] BULK DELETE: {Count} students deleted by {Admin}", count, AdminUsername());
            TempData["SuccessMessage"] = $"✅ تم حذف {count} طالب بنجاح";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDatabase(
            string confirmUsername,
            string confirmPassword)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var isValid = await _adminService.LoginAsync(confirmUsername, confirmPassword);
            if (!isValid)
            {
                TempData["ErrorMessage"] = "❌ اسم المستخدم أو كلمة المرور غير صحيحة";
                return RedirectToAction("Index");
            }

            var allStudents = await _studentService.GetAllStudentsAsync();
            int count = allStudents.Count();

            foreach (var student in allStudents.ToList())
                await _studentService.DeleteStudentAsync(student.Id, isAdmin: true);

            _logger.LogWarning("[AUDIT] DATABASE RESET: {Count} students deleted by {Admin}", count, AdminUsername());
            TempData["SuccessMessage"] = $"✅ تم إعادة ضبط قاعدة البيانات - تم حذف {count} طالب";
            return RedirectToAction("Index");
        }
    }
}
