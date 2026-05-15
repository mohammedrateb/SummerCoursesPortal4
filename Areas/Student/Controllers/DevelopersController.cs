using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Student.Controllers
{
    [Area("Student")]
    public class DevelopersController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
