using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Student;

namespace WebApplication1.Areas.Student.Controllers
{
    [Area("Student")]
    public class EditController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILevelRepository _levelRepo;
        private readonly IDivisionRepository _divisionRepo;

        public EditController(
            IStudentService studentService,
            ILevelRepository levelRepo,
            IDivisionRepository divisionRepo)
        {
            _studentService = studentService;
            _levelRepo = levelRepo;
            _divisionRepo = divisionRepo;
        }

        // GET - عرض فورم التعديل
        // ID يجي من Session مش من URL
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId = HttpContext.Session.GetInt32("SearchedStudentId");
            var verified = HttpContext.Session.GetString("SearchedStudentVerified");

            if (studentId == null || verified != "true")
                return RedirectToAction("Index", "Search", new { area = "Student" });

            var model = await _studentService.GetStudentForEditAsync(studentId.Value);

            if (model == null)
                return RedirectToAction("Index", "Search", new { area = "Student" });

            return View(model);
        }

        // POST - حفظ التعديلات بدون ظهور ID في URL
        [HttpPost]
        public async Task<IActionResult> Index(StudentEditViewModel model)
        {
            var studentId = HttpContext.Session.GetInt32("SearchedStudentId");
            var verified = HttpContext.Session.GetString("SearchedStudentVerified");

            if (studentId == null || verified != "true")
                return RedirectToAction("Index", "Search", new { area = "Student" });

            model.Id = studentId.Value;

            if (!ModelState.IsValid)
            {
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            var result = await _studentService.UpdateStudentAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "Search", new { area = "Student" });
        }
    }
}