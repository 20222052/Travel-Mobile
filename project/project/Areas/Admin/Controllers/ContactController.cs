using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContactController(AppDbContext context, IWebHostEnvironment env)
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
            ViewData["ContentSortParm"] = sortOrder == "content" ? "content_desc" : "content";

            var Query = _context.Contact.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => (u.Content != null && u.Content.Contains(searchString)) || 
                                       (u.Name != null && u.Name.Contains(searchString)) || 
                                       (u.Email != null && u.Email.Contains(searchString)));
            }

            Query = sortOrder switch
            {
                "id_desc" => Query.OrderByDescending(c => c.Id),
                "content" => Query.OrderBy(c => c.Content),
                "content_desc" => Query.OrderByDescending(c => c.Content),
                _ => Query.OrderBy(c => c.Id),
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
        public async Task<IActionResult> Create(Contact contact)
        {

            //User != null
            if (ModelState.IsValid)
            {
                _context.Add(contact);
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
            return View(contact);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.Contact.FindAsync(id);
            if (contact == null) return NotFound();

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (id != contact.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contact = await _context.Contact.FindAsync(id);
                if (contact == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy liên hệ cần xóa!";
                    return RedirectToAction("Index");
                }

                _context.Contact.Remove(contact);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa liên hệ thành công!";
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Xử lý lỗi khóa ngoại
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    TempData["ErrorMessage"] = "Không thể xóa liên hệ này vì đang được tham chiếu bởi dữ liệu khác!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa liên hệ. Vui lòng thử lại!";
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
