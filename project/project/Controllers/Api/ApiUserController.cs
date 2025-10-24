using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Policy = "AdminOrEditor")]
    public class ApiUserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ApiUserController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private static object ToDto(User u) => new
        {
            id = u.Id,
            username = u.Username,
            name = u.Name,
            email = u.Email,
            phone = u.Phone,
            role = u.Role,
            gender = u.Gender,
            address = u.Address,
            image = u.Image,
            dateOfBirth = u.DateOfBirth,
            createdDate = u.CreatedDate,
            otpVerified = u.OtpVerified,
        };

        // GET: api/ApiUser?page=1&pageSize=10&keyword=&role=
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            int page = 1, int pageSize = 10,
            string? keyword = "", string? role = "")
        {
            IQueryable<User> q = _context.User.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(u =>
                    u.Username.Contains(keyword) ||
                    (u.Email != null && u.Email.Contains(keyword)) ||
                    (u.Name != null && u.Name.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(role))
                q = q.Where(u => u.Role == role);

            q = q.OrderByDescending(u => u.Id);

            var totalCount = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new
            {
                items = items.Select(ToDto),
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                totalCount,
                page,
                pageSize
            });
        }

        // GET: api/ApiUser/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var u = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();
            return Ok(ToDto(u));
        }

        // POST: api/ApiUser  (multipart/form-data)
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Create([FromForm] UserCreateModel m)
        {
            // validate cơ bản
            if (string.IsNullOrWhiteSpace(m.Username) || string.IsNullOrWhiteSpace(m.Password))
                return BadRequest(new { message = "Username và Password là bắt buộc." });

            if (await _context.User.AnyAsync(u => u.Username == m.Username))
                return Conflict(new { message = "Username đã tồn tại." });

            if (!string.IsNullOrEmpty(m.Email) && await _context.User.AnyAsync(u => u.Email == m.Email))
                return Conflict(new { message = "Email đã được sử dụng." });

            var user = new User
            {
                Username = m.Username,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Role = string.IsNullOrEmpty(m.Role) ? "USER" : m.Role,
                Gender = m.Gender,
                Address = m.Address,
                DateOfBirth = m.DateOfBirth,
                OtpVerified = m.OtpVerified,
                CreatedDate = DateTime.Now
            };

            // upload ảnh
            if (m.ImageFile is not null && m.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "users");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(m.ImageFile.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(uploads, fileName));
                await m.ImageFile.CopyToAsync(stream);
                user.Image = "/uploads/users/" + fileName;
            }

            // hash password
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, m.Password);

            _context.User.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToDto(user));
        }

        // PUT: api/ApiUser/5 (multipart/form-data)
        [HttpPut("{id:int}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] UserUpdateModel m)
        {
            var u = await _context.User.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            // check email/phone trùng
            if (!string.IsNullOrWhiteSpace(m.Email) &&
                await _context.User.AnyAsync(x => x.Id != id && x.Email == m.Email))
                return Conflict(new { message = "Email đã tồn tại." });

            if (!string.IsNullOrWhiteSpace(m.Phone) &&
                await _context.User.AnyAsync(x => x.Id != id && x.Phone == m.Phone))
                return Conflict(new { message = "Số điện thoại đã tồn tại." });

            u.Name = m.Name ?? u.Name;
            u.Email = m.Email ?? u.Email;
            u.Phone = m.Phone ?? u.Phone;
            u.Role = m.Role ?? u.Role;
            u.Gender = m.Gender ?? u.Gender;
            u.Address = m.Address ?? u.Address;
            u.DateOfBirth = m.DateOfBirth ?? u.DateOfBirth;
            if (m.OtpVerified.HasValue) u.OtpVerified = m.OtpVerified.Value;

            // ảnh mới (nếu có)
            if (m.ImageFile is not null && m.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "users");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(m.ImageFile.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(uploads, fileName));
                await m.ImageFile.CopyToAsync(stream);
                u.Image = "/uploads/users/" + fileName;
            }

            // đổi mật khẩu (nếu có NewPassword)
            if (!string.IsNullOrWhiteSpace(m.NewPassword))
            {
                var hasher = new PasswordHasher<User>();
                u.Password = hasher.HashPassword(u, m.NewPassword);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/ApiUser/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.User.FindAsync(id);
            if (u is null) return NotFound();
            _context.User.Remove(u);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------- Binding models ----------
        public class UserCreateModel
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Role { get; set; } 
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public bool OtpVerified { get; set; } = false;

            public IFormFile? ImageFile { get; set; }
        }

        public class UserUpdateModel
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Role { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public bool? OtpVerified { get; set; }

            public string? NewPassword { get; set; } 
            public IFormFile? ImageFile { get; set; }
        }
    }
}
