using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Security.Claims;

namespace project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public AccountController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || user.Role != "USER")
            {
                TempData["Error"] = "Tài khoản không tồn tại hoặc không phải user";
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.Name ?? ""),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Avatar", user.Image ?? "")
                };

                var identity = new ClaimsIdentity(claims, "UserScheme");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("UserScheme", principal);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Mật khẩu không chính xác";
                return View(model);
            }
        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "USER")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return RedirectToAction("Index", "Home");
        }
        

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "USER")]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "USER")]
        public async Task<IActionResult> History()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            var orders = await _context.Order
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return View(orders);
        }
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (user.Password == null)
            {
                ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                return View(user);
            }
            // Kiểm tra trùng Username
            if (await _context.User.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(user);
            }

            // Kiểm tra trùng Email
            if (await _context.User.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(user);
            }

            // Kiểm tra trùng SĐT
            if (await _context.User.AnyAsync(u => u.Phone == user.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
                return View(user);
            }

            // Mã hóa mật khẩu
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            // Thêm vào database
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "USER")]
        public async Task<IActionResult> EditProfile()
        {
            // Lấy ID người dùng
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Truy vấn thông tin người dùng theo ID
            var useN = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (useN == null)
            {
                return NotFound();
            }

            // Truyền thông tin người dùng sang view
            return View(useN);
        }


        [Authorize(Roles = "USER")]
        [HttpPost]
        public async Task<IActionResult> EditProfile(User useN)
        {
            if (!ModelState.IsValid) return View(useN);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Kiểm tra email/phone trùng với user khác (nếu thay đổi)
            if (_context.User.Any(u => u.Id != userId && u.Email == useN.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(useN);
            }

            if (_context.User.Any(u => u.Id != userId && u.Phone == useN.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã được sử dụng");
                return View(useN);
            }

            // Cập nhật thông tin
            user.Name = useN.Name;
            user.Image = useN.Image;
            user.Gender = useN.Gender;
            user.Address = useN.Address;
            user.Phone = useN.Phone;
            user.Email = useN.Email;
            user.DateOfBirth = useN.DateOfBirth;
            if (useN.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(useN.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await useN.ImageFile.CopyToAsync(fileStream);
                }

               
                user.Image = uniqueFileName;
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.Name ?? ""),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Avatar", user.Image ?? "")
                };

            var identity = new ClaimsIdentity(claims, "UserScheme");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("UserScheme", principal);
            return RedirectToAction("Profile");
        }

    }
}
