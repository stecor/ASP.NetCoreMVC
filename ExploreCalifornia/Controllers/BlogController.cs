using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExploreCalifornia.Models;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExploreCalifornia.Controllers
{
    [Route("blog")]
    public class BlogController : Controller
    {
        private readonly BlogDataContext _db;

        public BlogController(BlogDataContext db)
        {
            _db = db;
        }

        [Route("")]
        // GET: /<controller>/
        public IActionResult Index(int page = 0)
        {
            // return new ContentResult { Content = "Blog posts"};
            //var posts = _db.Posts.OrderByDescending(x => x.Posted).Take(5).ToArray();

            var pageSize = 2;
            var totalPosts = _db.Posts.Count();
            var totalPages = totalPosts / pageSize;
            var previousPage = page - 1;
            var nextPage = page + 1;

            ViewBag.PreviousPage = previousPage;
            ViewBag.HasPreviousPage = previousPage >= 0;
            ViewBag.NextPage = nextPage;
            ViewBag.HasNextPage = nextPage < totalPages;

            var posts = _db.Posts.OrderByDescending(x=>x.Posted)
                                 .Skip(pageSize * page)
                                 .Take(pageSize)
                                 .ToArray();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(posts);

            return View(posts);
        }

        [Route("{year:min(2000)}/{month:range(1,12)}/{key}")]
        public IActionResult Post(int year, int month, string key)
        {

            // return new ContentResult {Content = string.Format("Year: {0}; Month: {1}; key: {2}", year, month, key)};
            var post = _db.Posts.FirstOrDefault(x => x.Key == key);

            //ViewBag.Title = "My blog post";
            //ViewBag.Posted = DateTime.Now;
            //ViewBag.Author = "Stefano Corra";
            //ViewBag.Body = "This is a great blog post";
            return View(post);
        }

        [Authorize]
        [HttpGet,Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost,Route("create")]
        public IActionResult Create(Post post) 
        {
            if (!ModelState.IsValid)
                return View();

            post.Author = User.Identity.Name;
            post.Posted = DateTime.Now;

            _db.Posts.Add(post);
            _db.SaveChanges();

            return RedirectToAction("Post", "Blog", new
            {
                year = post.Posted.Year,
                month = post.Posted.Month,
                key = post.Key
            });
        }

    }
}
