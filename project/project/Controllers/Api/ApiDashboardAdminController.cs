using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN,EDITOR")]
    public class ApiDashboardAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiDashboardAdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/ApiDashboardAdmin/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
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

                var result = new
                {
                    TotalOrders = totalOrders,
                    NewOrders = newOrders,
                    TotalUsers = totalUsers,
                    NewUsers = newUsers,
                    TotalTours = totalTours,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê dashboard.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/orders-per-month
        [HttpGet("orders-per-month")]
        public async Task<IActionResult> GetOrdersPerMonth([FromQuery] int months = 6)
        {
            try
            {
                var startDate = DateTime.Now.AddMonths(-months);
                var ordersPerMonth = await _context.Order
                    .Where(o => o.Date >= startDate)
                    .GroupBy(o => new { o.Date.Value.Year, o.Date.Value.Month })
                    .Select(g => new
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Count = g.Count(),
                        MonthYear = $"{g.Key.Month}/{g.Key.Year}"
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                return Ok(ordersPerMonth);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê đơn hàng theo tháng.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/orders-by-status
        [HttpGet("orders-by-status")]
        public async Task<IActionResult> GetOrdersByStatus()
        {
            try
            {
                var ordersByStatus = await _context.Order
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        StatusName = GetStatusName(g.Key),
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(ordersByStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê đơn hàng theo trạng thái.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/revenue-statistics
        [HttpGet("revenue-statistics")]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var startOfYear = new DateTime(DateTime.Now.Year, 1, 1);

                // Doanh thu tháng này - tính từ OrderDetail
                var monthlyRevenue = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Tour)
                    .Where(od => od.Order.Date >= startOfMonth && od.Order.Status == 3)
                    .SumAsync(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0));

                // Doanh thu năm nay
                var yearlyRevenue = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Tour)
                    .Where(od => od.Order.Date >= startOfYear && od.Order.Status == 3)
                    .SumAsync(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0));

                // Tổng doanh thu
                var totalRevenue = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Tour)
                    .Where(od => od.Order.Status == 3)
                    .SumAsync(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0));

                var result = new
                {
                    MonthlyRevenue = monthlyRevenue,
                    YearlyRevenue = yearlyRevenue,
                    TotalRevenue = totalRevenue
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê doanh thu.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/revenue-per-month
        [HttpGet("revenue-per-month")]
        public async Task<IActionResult> GetRevenuePerMonth([FromQuery] int months = 12)
        {
            try
            {
                var startDate = DateTime.Now.AddMonths(-months);
                var revenuePerMonth = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Tour)
                    .Where(od => od.Order.Date >= startDate && od.Order.Status == 3)
                    .GroupBy(od => new { od.Order.Date.Value.Year, od.Order.Date.Value.Month })
                    .Select(g => new
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0)),
                        OrderCount = g.Select(od => od.OrderId).Distinct().Count(),
                        MonthYear = $"{g.Key.Month}/{g.Key.Year}"
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                return Ok(revenuePerMonth);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê doanh thu theo tháng.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/top-tours
        [HttpGet("top-tours")]
        public async Task<IActionResult> GetTopTours([FromQuery] int limit = 10)
        {
            try
            {
                var topTours = await _context.OrderDetail
                    .Include(od => od.Tour)
                    .GroupBy(od => new { od.TourId, od.Tour.Name, od.Tour.Image })
                    .Select(g => new
                    {
                        TourId = g.Key.TourId,
                        TourName = g.Key.Name,
                        TourImage = g.Key.Image,
                        TotalBookings = g.Sum(od => od.Quantity ?? 0),
                        TotalRevenue = g.Sum(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0))
                    })
                    .OrderByDescending(x => x.TotalBookings)
                    .Take(limit)
                    .ToListAsync();

                return Ok(topTours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách tour phổ biến.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/recent-orders
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                var recentOrders = await _context.Order
                    .Include(o => o.User)
                    .Include(o => o.OrderDetail)
                        .ThenInclude(od => od.Tour)
                    .OrderByDescending(o => o.Date)
                    .Take(limit)
                    .ToListAsync();

                var result = recentOrders.Select(o => new
                {
                    o.Id,
                    o.Date,
                    TotalPrice = o.OrderDetail?.Sum(od => (od.Quantity ?? 0) * (od.Tour?.Price ?? 0)) ?? 0,
                    o.Status,
                    StatusName = GetStatusName(o.Status),
                    UserName = o.User?.Name,
                    UserEmail = o.User?.Email
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng gần đây.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/recent-users
        [HttpGet("recent-users")]
        public async Task<IActionResult> GetRecentUsers([FromQuery] int limit = 10)
        {
            try
            {
                var recentUsers = await _context.User
                    .Where(u => u.Role == "USER")
                    .OrderByDescending(u => u.CreatedDate)
                    .Take(limit)
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        u.Username,
                        u.Phone,
                        u.CreatedDate,
                        u.Image
                    })
                    .ToListAsync();

                return Ok(recentUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách người dùng mới.", error = ex.Message });
            }
        }

        // GET: /api/ApiDashboardAdmin/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);

                // Thống kê tổng quan
                var totalOrders = await _context.Order.CountAsync();
                var newOrders = await _context.Order.Where(o => o.Date >= startOfMonth).CountAsync();
                var totalUsers = await _context.User.Where(u => u.Role == "USER").CountAsync();
                var newUsers = await _context.User.Where(u => u.Role == "USER" && u.CreatedDate >= startOfMonth).CountAsync();
                var totalTours = await _context.Tour.CountAsync();
                var pendingOrders = await _context.Order.Where(o => o.Status == 0).CountAsync();
                var completedOrders = await _context.Order.Where(o => o.Status == 3 && o.Date >= startOfMonth).CountAsync();
                var cancelledOrders = await _context.Order.Where(o => o.Status == 4 && o.Date >= startOfMonth).CountAsync();

                // Đơn hàng theo tháng (6 tháng gần nhất)
                var ordersPerMonth = await _context.Order
                    .Where(o => o.Date >= sixMonthsAgo)
                    .GroupBy(o => new { o.Date.Value.Year, o.Date.Value.Month })
                    .Select(g => new
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Count = g.Count(),
                        MonthYear = $"{g.Key.Month}/{g.Key.Year}"
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                // Đơn hàng theo trạng thái
                var ordersByStatus = await _context.Order
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        StatusName = GetStatusName(g.Key),
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Doanh thu
                var monthlyRevenue = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Tour)
                    .Where(od => od.Order.Date >= startOfMonth && od.Order.Status == 3)
                    .SumAsync(od => (od.Quantity ?? 0) * (od.Tour.Price ?? 0));

                var result = new
                {
                    Statistics = new
                    {
                        TotalOrders = totalOrders,
                        NewOrders = newOrders,
                        TotalUsers = totalUsers,
                        NewUsers = newUsers,
                        TotalTours = totalTours,
                        PendingOrders = pendingOrders,
                        CompletedOrders = completedOrders,
                        CancelledOrders = cancelledOrders,
                        MonthlyRevenue = monthlyRevenue
                    },
                    OrdersPerMonth = ordersPerMonth,
                    OrdersByStatus = ordersByStatus
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy tổng quan dashboard.", error = ex.Message });
            }
        }

        // Helper method để lấy tên trạng thái
        private string GetStatusName(int? status)
        {
            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đã xác nhận",
                2 => "Đang xử lý",
                3 => "Hoàn thành",
                4 => "Đã hủy",
                _ => "Không xác định"
            };
        }
    }
}
