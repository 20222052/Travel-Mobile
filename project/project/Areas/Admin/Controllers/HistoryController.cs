using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class HistoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HistoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            int pageSize = 6;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["UserSortParm"] = sortOrder == "user" ? "user_desc" : "user";
            ViewData["TourSortParm"] = sortOrder == "tour" ? "tour_desc" : "tour";

            var Query = _context.History
                .Include(t => t.User) //Include để có dữ liệu User
                .Include(t => t.Tour) //Include để có dữ liệu Tour
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => (u.Content != null && u.Content.Contains(searchString)) || 
                                       (u.User != null && u.User.Name != null && u.User.Name.Contains(searchString)) || 
                                       (u.Tour != null && u.Tour.Name != null && u.Tour.Name.Contains(searchString)));
            }

            Query = sortOrder switch
            {
                "id_desc" => Query.OrderByDescending(h => h.Id),
                "user" => Query.OrderBy(h => h.User != null ? h.User.Name : ""),
                "user_desc" => Query.OrderByDescending(h => h.User != null ? h.User.Name : ""),
                "tour" => Query.OrderBy(h => h.Tour != null ? h.Tour.Name : ""),
                "tour_desc" => Query.OrderByDescending(h => h.Tour != null ? h.Tour.Name : ""),
                _ => Query.OrderBy(h => h.Id),
            };

            int total = await Query.CountAsync();

            var history = await Query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            return View(history);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(History history)
        {

            //User != null
            if (ModelState.IsValid)
            {
                _context.Add(history);
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
            return View(history);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var history = await _context.History.FindAsync(id);
            if (history == null) return NotFound();

            return View(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, History history)
        {
            if (id != history.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(history);
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
            return View(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var history = await _context.History.FindAsync(id);
                if (history == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch sử cần xóa!";
                    return RedirectToAction("Index");
                }

                _context.History.Remove(history);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa lịch sử thành công!";
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Xử lý lỗi khóa ngoại
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    TempData["ErrorMessage"] = "Không thể xóa lịch sử này vì đang được tham chiếu bởi dữ liệu khác!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lịch sử. Vui lòng thử lại!";
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
