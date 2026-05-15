using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Student;

namespace WebApplication1.Areas.Student.Controllers
{
    [Area("Student")]
    public class RegisterController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILevelRepository _levelRepo;
        private readonly IDivisionRepository _divisionRepo;
        private readonly IRegistrationConfigService _regConfigService;

        public RegisterController(
            IStudentService studentService,
            ILevelRepository levelRepo,
            IDivisionRepository divisionRepo,
            IRegistrationConfigService regConfigService)
        {
            _studentService = studentService;
            _levelRepo = levelRepo;
            _divisionRepo = divisionRepo;
            _regConfigService = regConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var config = await _regConfigService.GetConfigAsync();
            if (!config.IsRegistrationOpen())
            {
                ViewBag.ClosedMessage = config.ClosedMessage;
                return View("Closed");
            }

            var model = new StudentRegisterViewModel
            {
                Levels = await _levelRepo.GetAllLevelsAsync(),
                Divisions = await _divisionRepo.GetAllDivisionsAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(StudentRegisterViewModel model)
        {
            var config = await _regConfigService.GetConfigAsync();
            if (!config.IsRegistrationOpen())
            {
                ViewBag.ClosedMessage = config.ClosedMessage;
                return View("Closed");
            }

            if (!ModelState.IsValid)
            {
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            var result = await _studentService.RegisterStudentAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.Levels = await _levelRepo.GetAllLevelsAsync();
                model.Divisions = await _divisionRepo.GetAllDivisionsAsync();
                return View(model);
            }

            TempData["AccessCode"] = result.Message;
            TempData["StudentName"] = model.FullName;
            TempData["NationalId"] = model.NationalId;
            return RedirectToAction("Success");
        }

        [HttpGet]
        public IActionResult Success()
        {
            var code = TempData["AccessCode"]?.ToString();
            if (string.IsNullOrEmpty(code))
                return RedirectToAction("Index");

            ViewBag.AccessCode = code;
            ViewBag.StudentName = TempData["StudentName"]?.ToString();
            ViewBag.NationalId = TempData["NationalId"]?.ToString();
            return View();
        }
    }
}
