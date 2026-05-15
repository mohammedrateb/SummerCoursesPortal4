using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels.Admin;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IAdminService _adminService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostService postService,
            IAdminService adminService,
            ILogger<PostsController> logger)
        {
            _postService = postService;
            _adminService = adminService;
            _logger = logger;
        }

        private bool IsAdminLoggedIn() =>
            HttpContext.Session.GetString("IsAdminLoggedIn") == "true";

        // ── عرض كل المنشورات ──
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var posts = await _postService.GetAllPostsAsync();
            return View(posts);
        }

        // ── إنشاء منشور - GET ──
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            return View(new CreatePostViewModel());
        }

        // ── إنشاء منشور - POST ──
        [HttpPost]
        [RequestSizeLimit(209715200)] // 200MB
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _postService.CreatePostAsync(model);
                TempData["SuccessMessage"] = "✅ تم نشر المنشور بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل إنشاء المنشور");
                TempData["ErrorMessage"] = "❌ حصل خطأ أثناء النشر. حاول مرة أخرى أو تحقق من حجم/نوع الملفات.";
                return View(model);
            }
        }

        // ── تعديل منشور - GET ──
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
                return RedirectToAction("Index");

            var model = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ExistingAttachments = post.Attachments.ToList()
            };

            return View(model);
        }

        // ── تعديل منشور - POST ──
        [HttpPost]
        [RequestSizeLimit(209715200)] // 200MB
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Edit(EditPostViewModel model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _postService.UpdatePostAsync(model);
                TempData["SuccessMessage"] = "✅ تم تعديل المنشور بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل تعديل المنشور {Id}", model.Id);
                TempData["ErrorMessage"] = "❌ حصل خطأ أثناء التعديل. حاول مرة أخرى.";
                return View(model);
            }
        }

        // ── حذف منشور ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            await _postService.DeletePostAsync(id);
            TempData["SuccessMessage"] = "✅ تم حذف المنشور";
            return RedirectToAction("Index");
        }

        // ── حذف كل المنشورات ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(
            string confirmUsername,
            string confirmPassword)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var isValid = await _adminService.LoginAsync(
                confirmUsername, confirmPassword);

            if (!isValid)
            {
                TempData["ErrorMessage"] = "❌ بيانات الدخول غير صحيحة";
                return RedirectToAction("Index");
            }

            await _postService.DeleteAllPostsAsync();
            TempData["SuccessMessage"] = "✅ تم حذف كل المنشورات";
            return RedirectToAction("Index");
        }
    }
}