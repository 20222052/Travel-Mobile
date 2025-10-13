using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            int pageSize = 6;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TypeSortParm"] = string.IsNullOrEmpty(sortOrder) ? "type_desc" : "";
            ViewData["IdSortParm"] = sortOrder == "id" ? "id_desc" : "id";

            var Query = _context.Category.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => u.Type.ToLower().Contains(searchString.ToLower()));
            }

            Query = sortOrder switch
            {
                "type_desc" => Query.OrderByDescending(c => c.Type),
                "id" => Query.OrderBy(c => c.Id),
                "id_desc" => Query.OrderByDescending(c => c.Id),
                _ => Query.OrderBy(c => c.Type),
            };

            int totalUsers = await Query.CountAsync();

            var cates = await Query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)pageSize);
            return View(cates);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category cate)
        {
            
            //User != null
            if (ModelState.IsValid)
            {
                _context.Add(cate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành công!";
                return RedirectToAction("Index");
            }
            //foreach (var item in ModelState)
            //{
            //    foreach (var error in item.Value.Errors)
            //    {
            //        Console.WriteLine($"[ModelState] {item.Key} => {error.ErrorMessage}");
            //    }
            //}
            return View(cate);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cate = await _context.Category.FindAsync(id);
            if (cate == null) return NotFound();

            return View(cate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category cate)
        {
            if (id != cate.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(cate);           
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
            return View(cate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cate = await _context.Category.FindAsync(id);
                if (cate == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục cần xóa!";
                    return RedirectToAction("Index");
                }

                _context.Category.Remove(cate);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Xử lý lỗi khóa ngoại
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đang được sử dụng bởi Tour hoặc Blog. Vui lòng xóa các Tour/Blog liên quan trước!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh mục. Vui lòng thử lại!";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
