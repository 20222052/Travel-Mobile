using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Security.Claims;

namespace project.Controllers
{
    [Route("api/[controller]")]                 // => /api/ApiAccountAdmin/...
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiAccountAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiAccountAdminController(AppDbContext context)
        {
            _context = context;
        }

        // POST: /api/ApiAccountAdmin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            // Chỉ cho ADMIN/EDITOR vào khu vực quản trị
            if (user == null || (user.Role != "ADMIN" && user.Role != "EDITOR"))
                return Unauthorized(new { message = "Không có quyền quản trị." });

            var hasher = new PasswordHasher<User>();
            var verify = hasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (verify != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role),           // ADMIN / EDITOR
                new("FullName", user.Name ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, "AdminScheme");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminScheme", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

            return Ok(new
            {
                message = "Đăng nhập quản trị thành công.",
                user = new { user.Id, user.Username, user.Name, user.Role }
            });
        }

        // GET: /api/ApiAccountAdmin/userinfo
        [Authorize(AuthenticationSchemes = "AdminScheme")]
        [HttpGet("userinfo")]
        public async Task<IActionResult> UserInfo()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id)) return BadRequest(new { message = "ID không hợp lệ." });

            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                name = user.Name,
                role = user.Role,
                email = user.Email,
                image = user.Image
            });
        }

        // POST: /api/ApiAccountAdmin/logout
        [Authorize(AuthenticationSchemes = "AdminScheme")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminScheme");
            return Ok(new { message = "Đăng xuất quản trị thành công." });
        }
    }
}

