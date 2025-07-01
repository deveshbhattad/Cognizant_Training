using Bloggie.MVC.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Bloggie.MVC.Controllers
    
{

    public class BlogController : Controller
    {
        private IBlogPostRepository blogPostRepository;

        public BlogController(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository;
        }
        [HttpGet]
        public async Task< IActionResult> Index(string Urlhandle)
        {
            // Return a default view to ensure all code paths return a value
            var blogposts = await blogPostRepository.GetByUrlHandleAsync(Urlhandle);
            return View(blogposts);
        }
    }
}
