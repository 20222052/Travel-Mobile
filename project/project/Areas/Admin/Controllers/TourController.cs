using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Threading.Tasks;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class TourController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TourController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            int pageSize = 4;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["LocationSortParm"] = sortOrder == "location" ? "location_desc" : "location";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DurationSortParm"] = sortOrder == "duration" ? "duration_desc" : "duration";

            var Query = _context.Tour
                .Include(t => t.Category) //Include để có dữ liệu
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => u.Name.Contains(searchString) || u.Location.Contains(searchString));
            }

            Query = sortOrder switch
            {
                "name_desc" => Query.OrderByDescending(t => t.Name),
                "location" => Query.OrderBy(t => t.Location),
                "location_desc" => Query.OrderByDescending(t => t.Location),
                "price" => Query.OrderBy(t => t.Price),
                "price_desc" => Query.OrderByDescending(t => t.Price),
                "duration" => Query.OrderBy(t => t.Duration),
                "duration_desc" => Query.OrderByDescending(t => t.Duration),
                _ => Query.OrderBy(t => t.Name),
            };

            int total = await Query.CountAsync();

            var tour = await Query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            return View(tour);
        }


        public async Task<IActionResult> Create()
        {

            ViewBag.Category = await _context.Category.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour)
        {
            if (ModelState.IsValid)
            {
                if (tour.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(tour.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await tour.ImageFile.CopyToAsync(fileStream);
                    }

                    tour.Image = uniqueFileName;
                }
                _context.Add(tour);
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
            return View(tour);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tour.FindAsync(id);
            if (tour == null) return NotFound();
            var category = await _context.Category.ToListAsync();
            ViewBag.Category = category;
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour tour)
        {
            if (id != tour.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingTour = await _context.Tour.FindAsync(id);
                if (existingTour == null)
                    return NotFound();

                // Cập nhật các trường
                existingTour.Name = tour.Name;
                existingTour.CategoryId = tour.CategoryId;
                existingTour.Location = tour.Location;
                existingTour.Duration = tour.Duration;
                existingTour.Price = tour.Price;
                existingTour.People = tour.People;
                existingTour.Description = tour.Description;
                //Xử lý ảnh
                if (tour.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(tour.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await tour.ImageFile.CopyToAsync(fileStream);
                    }

                    // Nếu có ảnh cũ thì xóa
                    if (!string.IsNullOrEmpty(existingTour.Image))
                    {
                        string oldFile = Path.Combine(uploadsFolder, existingTour.Image);
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);
                    }

                    existingTour.Image = uniqueFileName;
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
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tour = await _context.Tour.FindAsync(id);
                if (tour == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tour cần xóa!";
                    return RedirectToAction("Index");
                }

                _context.Tour.Remove(tour);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa tour thành công!";
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Xử lý lỗi khóa ngoại
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    TempData["ErrorMessage"] = "Không thể xóa tour này vì đang có đơn đặt hàng liên quan. Vui lòng xóa các đơn hàng trước!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa tour. Vui lòng thử lại!";
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
