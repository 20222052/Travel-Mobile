using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class UserController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 6;

            ViewData["CurrentFilter"] = searchString;

            var usersQuery = _context.User.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.Name.Contains(searchString) || u.Email.Contains(searchString) || u.Username.Contains(searchString) || u.Phone.Contains(searchString));
            }

            int totalUsers = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)pageSize);
            return View(users);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (_context.User.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }
            if (_context.User.Any(u => u.Phone == user.Phone))
            {
                ModelState.AddModelError("Phone", "SĐT đã tồn tại.");

            }
            if (_context.User.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username đã tồn tại.");
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu là không được để trống.");
            }
            //User != null
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, user.Password);
                if (user.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder); 

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }

                    user.Image = uniqueFileName;
                }
                _context.Add(user);
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
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingUser = await _context.User.FindAsync(id);
                if (existingUser == null)
                    return NotFound();

                // Nếu không nhập password mới → giữ lại password cũ
                if (string.IsNullOrEmpty(user.Password))
                {
                    user.Password = existingUser.Password;
                }

                // Cập nhật các trường
                existingUser.Name = user.Name;
                existingUser.Username = user.Username;
                existingUser.Gender = user.Gender;
                existingUser.Address = user.Address;
                existingUser.Phone = user.Phone;
                existingUser.Email = user.Email;
                existingUser.DateOfBirth = user.DateOfBirth;
                existingUser.Role = user.Role;
                var hasher = new PasswordHasher<User>();
                existingUser.Password = hasher.HashPassword(user, user.Password);
                //Xử lý ảnh
                if (user.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }

                    // Nếu có ảnh cũ thì xóa
                    if (!string.IsNullOrEmpty(existingUser.Image))
                    {
                        string oldFile = Path.Combine(uploadsFolder, existingUser.Image);
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);
                    }

                    existingUser.Image = uniqueFileName;
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
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
