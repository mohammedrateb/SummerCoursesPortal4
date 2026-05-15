using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Admin;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly IAdminService _adminService;

        public AuthController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") == "true")
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            return View();
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isValid = await _adminService.LoginAsync(model.Username, model.Password);

            if (!isValid)
            {
                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
                return View(model);
            }

            // إعادة توليد session لمنع session fixation
            await HttpContext.Session.LoadAsync();
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("IsAdminLoggedIn", "true");
            HttpContext.Session.SetString("AdminUsername", model.Username);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
                return RedirectToAction("Login");
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var username = HttpContext.Session.GetString("AdminUsername");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var ok = await _adminService.ChangePasswordAsync(username, model.CurrentPassword, model.NewPassword);
            if (!ok)
            {
                ModelState.AddModelError("", "كلمة المرور الحالية غير صحيحة");
                return View(model);
            }

            TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح";
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}
