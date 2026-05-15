using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Areas.Student.Controllers
{
    [Area("Student")]
    public class NewsController : Controller
    {
        private readonly IPostService _postService;

        public NewsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();
            return View(posts.Where(p => p.IsPublished));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null || !post.IsPublished)
                return RedirectToAction("Index");

            return View(post);
        }
    }
}