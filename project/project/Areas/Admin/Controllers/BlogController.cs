using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 6;

            ViewData["CurrentFilter"] = searchString;

            var Query = _context.Blog
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => u.Title.Contains(searchString) || u.Content.Contains(searchString));
            }

            int totalUsers = await Query.CountAsync();

            var blog = await Query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)pageSize);
            return View(blog);
        }


        public IActionResult Create()
        {
            ViewBag.Category = _context.Category.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                if (blog.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(blog.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await blog.ImageFile.CopyToAsync(fileStream);
                    }

                    blog.Image = uniqueFileName;
                }
                _context.Add(blog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                return RedirectToAction("Index");
            }
            //foreach (var item in ModelState)
            //{
            //    foreach (var error in item.Value.Errors)
            //    {
            //        Console.WriteLine($"[ModelState] {item.Key} => {error.ErrorMessage}");
            //    }
            //}
            return View(blog);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blog.FindAsync(id);
            if (blog == null) return NotFound();
            ViewBag.Category = _context.Category.ToList();
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingBlog = await _context.Blog.FindAsync(id);
                if (existingBlog == null)
                    return NotFound();
                // Cập nhật các trường
                existingBlog.Title = blog.Title;
                existingBlog.Content = blog.Content;
                existingBlog.CategoryId = blog.CategoryId;
                //Xử lý ảnh
                if (blog.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(blog.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await blog.ImageFile.CopyToAsync(fileStream);
                    }

                    // Nếu có ảnh cũ thì xóa
                    if (!string.IsNullOrEmpty(existingBlog.Image))
                    {
                        string oldFile = Path.Combine(uploadsFolder, existingBlog.Image);
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);
                    }

                    existingBlog.Image = uniqueFileName;
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.User.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blog.FindAsync(id);
            if (blog != null)
            {
                _context.Blog.Remove(blog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
