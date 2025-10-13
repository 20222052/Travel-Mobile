using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.OtpConfig;
using System.Security.Claims;

namespace project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class ApiAccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;

        public ApiAccountController(AppDbContext context, IOtpService otpService)
        {
            _context = context;
            _otpService = otpService;
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

            // Kiểm tra tài khoản đã xác thực OTP chưa
            if (!user.OtpVerified)
            {
                return Unauthorized(new 
                { 
                    message = "Tài khoản chưa được xác thực. Vui lòng kiểm tra email và xác thực OTP.",
                    email = user.Email,
                    requireOtpVerification = true
                });
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
                CreatedDate = DateTime.Now,
                OtpVerified = false // Tài khoản chưa được xác thực
            };

            // Hash mật khẩu
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.Password);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // Tạo và gửi OTP qua email
            try
            {
                var otpRequest = new Models.Request.OtpRequest { Email = model.Email };
                var otpResult = await _otpService.GenerateOtpAsync(otpRequest);

                if (!otpResult.Success)
                {
                    return Ok(new 
                    { 
                        message = "Đăng ký thành công nhưng không thể gửi OTP. Vui lòng yêu cầu gửi lại OTP.",
                        email = model.Email,
                        otpSent = false
                    });
                }

                return Ok(new 
                { 
                    message = "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP xác thực tài khoản.",
                    email = model.Email,
                    otpSent = true
                });
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    message = "Đăng ký thành công nhưng gặp lỗi khi gửi OTP: " + ex.Message,
                    email = model.Email,
                    otpSent = false
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] Models.Request.VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
                return BadRequest(new { message = "Email và mã OTP không được để trống." });

            // Kiểm tra user có tồn tại không
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản với email này." });

            // Kiểm tra user đã xác thực chưa
            if (user.OtpVerified)
                return BadRequest(new { message = "Tài khoản đã được xác thực trước đó." });

            // Xác thực OTP
            var otpResult = await _otpService.VerifyOtpAsync(request);

            if (!otpResult.Success)
                return BadRequest(new { message = otpResult.Message });

            // Cập nhật trạng thái xác thực
            user.OtpVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Xác thực OTP thành công! Tài khoản đã được kích hoạt.",
                verified = true
            });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] Models.Request.OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Email không được để trống." });

            // Kiểm tra user có tồn tại không
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản với email này." });

            // Kiểm tra user đã xác thực chưa
            if (user.OtpVerified)
                return BadRequest(new { message = "Tài khoản đã được xác thực." });

            // Tạo và gửi OTP mới
            var otpResult = await _otpService.GenerateOtpAsync(request);

            if (!otpResult.Success)
                return BadRequest(new { message = otpResult.Message });

            return Ok(new { message = "Đã gửi lại mã OTP qua email." });
        }

        [Authorize(AuthenticationSchemes = "UserScheme")]
        [HttpPut("editProfile")]
        public async Task<IActionResult> EditProfile([FromForm] User model, IFormFile? ImageFile)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { message = "Chưa đăng nhập." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest(new { message = "ID không hợp lệ." });

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            try
            {
                // Cập nhật thông tin
                if (!string.IsNullOrWhiteSpace(model.Name))
                    user.Name = model.Name;

                if (!string.IsNullOrWhiteSpace(model.Gender))
                    user.Gender = model.Gender;

                if (!string.IsNullOrWhiteSpace(model.Phone))
                    user.Phone = model.Phone;

                if (!string.IsNullOrWhiteSpace(model.Address))
                    user.Address = model.Address;

                if (!string.IsNullOrWhiteSpace(model.Email))
                    user.Email = model.Email;

                if (model.DateOfBirth.HasValue)
                    user.DateOfBirth = model.DateOfBirth;

                // Xử lý upload ảnh đại diện
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(user.Image))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, user.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    user.Image = uniqueFileName;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật thông tin thành công.",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        name = user.Name,
                        gender = user.Gender,
                        email = user.Email,
                        phone = user.Phone,
                        image = user.Image,
                        address = user.Address,
                        dateOfBirth = user.DateOfBirth,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật thông tin.", error = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = "UserScheme")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return Ok(new { message = "Đăng xuất thành công." });
        }

    }
}

