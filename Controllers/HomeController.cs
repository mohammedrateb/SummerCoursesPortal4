using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "Student" });
        }

        [Route("Home/Error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            ViewBag.StatusCode = statusCode ?? 500;
            ViewBag.Title = statusCode switch
            {
                404 => "الصفحة غير موجودة",
                403 => "غير مصرح",
                429 => "طلبات كثيرة جداً",
                500 => "خطأ في الخادم",
                _ => "حدث خطأ"
            };
            ViewBag.Message = statusCode switch
            {
                404 => "الصفحة التي تبحث عنها غير موجودة أو تم نقلها.",
                403 => "ليس لديك صلاحية الوصول لهذه الصفحة.",
                429 => "تجاوزت الحد المسموح من الطلبات. يرجى الانتظار دقيقة والمحاولة مرة أخرى.",
                500 => "حدث خطأ داخلي في الخادم. يرجى المحاولة مرة أخرى.",
                _ => "حدث خطأ غير متوقع."
            };
            return View("Error");
        }
    }
}
