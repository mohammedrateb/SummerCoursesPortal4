using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Student;

namespace WebApplication1.Areas.Student.Controllers
{
    [Area("Student")]
    public class SearchController : Controller
    {
        private readonly IStudentService _studentService;

        public SearchController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST - البحث بالرقم القومى + كود الوصول
        [HttpPost]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> Index(string nationalId, string accessCode)
        {
            if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 14)
            {
                ViewBag.Error = "الرقم القومي يجب أن يكون 14 رقم";
                return View();
            }

            if (string.IsNullOrWhiteSpace(accessCode) || accessCode.Trim().Length < 6)
            {
                ViewBag.Error = "كود الوصول مطلوب (8 أحرف). تجده فى رسالة التسجيل.";
                ViewBag.NationalId = nationalId;
                return View();
            }

            var student = await _studentService.GetStudentByNationalIdAsync(nationalId);

            if (student == null)
            {
                ViewBag.NotFound = true;
                ViewBag.NationalId = nationalId;
                return View();
            }

            // التحقق من كود الوصول (مش بنفرّق بين "مش موجود" و"كود خطأ" لمنع enumeration)
            var codeOk = await _studentService.VerifyAccessCodeAsync(student.Id, accessCode);
            if (!codeOk)
            {
                ViewBag.Error = "بيانات غير صحيحة. تأكد من الرقم القومى وكود الوصول.";
                ViewBag.NationalId = nationalId;
                return View();
            }

            HttpContext.Session.SetInt32("SearchedStudentId", student.Id);
            HttpContext.Session.SetString("SearchedStudentVerified", "true");
            TempData["StudentData"] = System.Text.Json.JsonSerializer.Serialize(student);

            return RedirectToAction("Result");
        }

        [HttpGet]
        public IActionResult Result()
        {
            var data = TempData["StudentData"]?.ToString();
            if (string.IsNullOrEmpty(data))
                return RedirectToAction("Index");

            var student = System.Text.Json.JsonSerializer.Deserialize<StudentDetailsViewModel>(data);
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var studentId = HttpContext.Session.GetInt32("SearchedStudentId");
            var verified = HttpContext.Session.GetString("SearchedStudentVerified");

            if (studentId == null || verified != "true")
            {
                TempData["ErrorMessage"] = "انتهت الجلسة، رجاءً ابحث من جديد.";
                return RedirectToAction("Index");
            }

            var result = await _studentService.DeleteStudentAsync(studentId.Value);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            HttpContext.Session.Remove("SearchedStudentId");
            HttpContext.Session.Remove("SearchedStudentVerified");
            TempData["SuccessMessage"] = "تم حذف البيانات بنجاح";
            return RedirectToAction("Index");
        }
    }
}
