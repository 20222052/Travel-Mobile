using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace project.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Tour = await _context.Tour.ToListAsync();
            // Lấy ngẫu nhiên 4 tour
            ViewBag.RandomTours = await _context.Tour
                .OrderBy(t => Guid.NewGuid())
                .Include(t => t.Category)
                .Take(4)
                .ToListAsync();

            // Lấy 3 tour có cột View lớn nhất
            ViewBag.TopViewedTours = await _context.Tour
                .OrderByDescending(t => t.View)
                .Include(t => t.Category)
                .Take(3)
                .ToListAsync();

            // Lấy tour mới nhất theo CreatedDate
            ViewBag.LatestTour = await _context.Tour
                .OrderByDescending(t => t.CreatedDate)
                .Include(t => t.Category)
                .FirstOrDefaultAsync();

            ViewBag.TourCate = await _context.Tour
                .Include(t => t.Category)
                .Where(t => t.CategoryId == 5)
                .OrderByDescending(t => t.CreatedDate)
                .Take(3)
                .ToListAsync();

            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        
        public async Task<IActionResult> Blog(string searchString, int cateId, int page = 1)
        {
            int pageSize = 6;
            ViewBag.category = await _context.Category
             .Select(c => new
             {
                 c.Id,
                 c.Type,
                 BlogCount = _context.Blog.Count(b => b.CategoryId == c.Id)
             })
             .ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = cateId;

            var Query = _context.Blog
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => u.Title.Contains(searchString) || u.Content.Contains(searchString));
            }

            if (cateId > 0)
            {
                Query = Query.Where(u => u.CategoryId == cateId);
            }

            int total = await Query.CountAsync();

            var blog = await Query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);

            return View(blog);
        }

        public async Task<IActionResult> BlogDetail(int id)
        {
            var blog = await _context.Blog
                .Include(t => t.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
            var blogPre = await _context.Blog
                .Include(t => t.Category)
                .Where(b => b.Id < id)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();  // null nếu không có bài trước

            var blogNext = await _context.Blog
                .Include(t => t.Category)
                .Where(b => b.Id > id)
                .OrderBy(b => b.Id)
                .FirstOrDefaultAsync();  // null nếu không có bài sau

            ViewBag.BlogPre = blogPre;
            ViewBag.BlogNext = blogNext;

            if (blog == null)
                return NotFound();

            return View(blog);
        }

        public async Task<IActionResult> Place(string searchString, int cateId, int page = 1)
        {
            int pageSize = 6;
            ViewBag.category = await _context.Category.ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = cateId;

            var Query = _context.Tour
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => u.Name.Contains(searchString) || u.Location.Contains(searchString));
            }

            if (cateId > 0)
            {
                Query = Query.Where(u => u.CategoryId == cateId);
            }

            int total = await Query.CountAsync();

            var tour = await Query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);

            return View(tour);
        }

        public async Task<IActionResult> TourDetail(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tour
                .Include(t => t.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (tour == null) return NotFound();
            return View(tour);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact contact)
        {
             _context.Add(contact);
             await _context.SaveChangesAsync();
             TempData["SuccessMessage"] = "Đã gửi yêu cầu của bạn.";
            //foreach (var item in ModelState)
            //{
            //    foreach (var error in item.Value.Errors)
            //    {
            //        Console.WriteLine($"[ModelState] {item.Key} => {error.ErrorMessage}");
            //    }
            //}
            return View();
        }
    }
}
