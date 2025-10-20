using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Security.Claims;

namespace project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiAccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ApiAccountController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || user.Role != "USER")
            {
                return Unauthorized(new { message = "Tài khoản không tồn tại hoặc không phải USER." });
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

                return Ok(new
                {
                    message = "Đăng nhập thành công.",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        name = user.Name,
                        role = user.Role,
                        image = user.Image
                    }
                });
            }
            else
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            }
        }


        [Authorize(AuthenticationSchemes = "UserScheme")]
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { message = "Chưa đăng nhập." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest(new { message = "ID không hợp lệ." });

            var user = await _context.User.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                name = user.Name,
                gender = user.Gender,
                email = user.Email,
                phone = user.Phone,
                image = user.Image,
                address = user.Address,
                createdDate = user.CreatedDate,
                dateOfBirth = user.DateOfBirth,
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra trùng username hoặc email
            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Username hoặc Email đã tồn tại." });
            }

            var user = new User
            {
                Name = model.Name,
                Gender = model.Gender,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email,
                Username = model.Username,
                Role = "USER",
                DateOfBirth = model.DateOfBirth,
                CreatedDate = DateTime.Now
            };

            // Hash mật khẩu
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.Password);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công." });
        }
        [Authorize(AuthenticationSchemes = "UserScheme")]
        [HttpPut("editProfile")]
        public async Task<IActionResult> EditProfile([FromForm] User useN)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (_context.User.Any(u => u.Id != userId && u.Email == useN.Email))
                return BadRequest(new { message = "Email đã được sử dụng" });

            if (_context.User.Any(u => u.Id != userId && u.Phone == useN.Phone))
                return BadRequest(new { message = "Số điện thoại đã được sử dụng" });

            user.Name = useN.Name;
            user.Gender = useN.Gender;
            user.Address = useN.Address;
            user.Phone = useN.Phone;
            user.Email = useN.Email;
            user.DateOfBirth = useN.DateOfBirth;

            if (useN.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(useN.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await useN.ImageFile.CopyToAsync(fileStream);
                }

                user.Image = uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", user });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                const string UserScheme = "UserScheme";
                await HttpContext.SignOutAsync(UserScheme);
                return Ok(new { message = "Đăng xuất thành công" }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi khi đăng xuất", error = ex.Message });
            }
        }

    }
}

