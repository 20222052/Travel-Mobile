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
        public async Task<IActionResult> Index()
        {
            // Tổng số đơn hàng
            var totalOrders = await _context.Order.CountAsync();
            
            // Đơn hàng mới (tháng này)
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var newOrders = await _context.Order
                .Where(o => o.Date >= startOfMonth)
                .CountAsync();
            
            // Tổng số người dùng
            var totalUsers = await _context.User
                .Where(u => u.Role == "USER")
                .CountAsync();
            
            // Người dùng mới (tháng này)
            var newUsers = await _context.User
                .Where(u => u.Role == "USER" && u.CreatedDate >= startOfMonth)
                .CountAsync();
            
            // Tổng số tour
            var totalTours = await _context.Tour.CountAsync();
            
            // Đơn hàng đang chờ xử lý
            var pendingOrders = await _context.Order
                .Where(o => o.Status == 0)
                .CountAsync();
            
            // Đơn hàng hoàn thành (tháng này)
            var completedOrders = await _context.Order
                .Where(o => o.Status == 3 && o.Date >= startOfMonth)
                .CountAsync();
            
            // Đơn hàng bị hủy (tháng này)
            var cancelledOrders = await _context.Order
                .Where(o => o.Status == 4 && o.Date >= startOfMonth)
                .CountAsync();
            
            // Đơn hàng theo tháng (6 tháng gần nhất)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var ordersPerMonth = await _context.Order
                .Where(o => o.Date >= sixMonthsAgo)
                .GroupBy(o => new { o.Date.Value.Year, o.Date.Value.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            
            // Đơn hàng theo trạng thái
            var ordersByStatus = await _context.Order
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
            
            // Gửi dữ liệu sang View
            ViewBag.TotalOrders = totalOrders;
            ViewBag.NewOrders = newOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.NewUsers = newUsers;
            ViewBag.TotalTours = totalTours;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.CancelledOrders = cancelledOrders;
            ViewBag.OrdersPerMonth = ordersPerMonth;
            ViewBag.OrdersByStatus = ordersByStatus;
            
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
