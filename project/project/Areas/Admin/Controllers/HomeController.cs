using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Security.Claims;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }  

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || user.Role == "USER")
            {
                TempData["Error"] = "Tài khoản không tồn tại";
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
                    new Claim("FullName", user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Avatar", user.Image ?? "")
                };

                var identity = new ClaimsIdentity(claims, "AdminScheme");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("AdminScheme", principal);

                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Mật khẩu không chính xác";
                return View(model);
            }
        }

        [Authorize(AuthenticationSchemes = "AdminScheme")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminScheme");
            return RedirectToAction("Login");
        }

    }
}
