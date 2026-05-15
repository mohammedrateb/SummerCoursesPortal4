using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly IRegistrationConfigService _regConfigService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            IRegistrationConfigService regConfigService,
            ILogger<SettingsController> logger)
        {
            _regConfigService = regConfigService;
            _logger = logger;
        }

        private bool IsAdminLoggedIn() =>
            HttpContext.Session.GetString("IsAdminLoggedIn") == "true";

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var config = await _regConfigService.GetConfigAsync();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationConfig model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var config = await _regConfigService.GetConfigAsync();

            // نقرأ IsOpen مباشرة من النموذج لتجنب مشكلة model binding مع hidden+checkbox
            config.IsOpen = Request.Form["IsOpen"].Any(v => v == "true");
            config.OpenAt = model.OpenAt;
            config.CloseAt = model.CloseAt;
            config.ClosedMessage = string.IsNullOrWhiteSpace(model.ClosedMessage)
                ? "التسجيل مغلق حالياً. يرجى متابعة الإعلانات."
                : model.ClosedMessage;

            await _regConfigService.UpdateConfigAsync(config);

            var admin = HttpContext.Session.GetString("AdminUsername") ?? "admin";
            _logger.LogInformation("[AUDIT] Registration config UPDATED by {Admin}: IsOpen={IsOpen}", admin, config.IsOpen);

            TempData["SuccessMessage"] = config.IsOpen
                ? "✅ تم فتح التسجيل بنجاح"
                : "🔒 تم إغلاق التسجيل بنجاح";

            return RedirectToAction("Index");
        }
    }
}
