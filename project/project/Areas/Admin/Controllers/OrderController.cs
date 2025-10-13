using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.Services;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public OrderController(AppDbContext context, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            int pageSize = 6;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["StatusSortParm"] = sortOrder == "status" ? "status_desc" : "status";

            var Query = _context.Order
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                Query = Query.Where(u => (u.Name != null && u.Name.Contains(searchString)) || 
                                       (u.Email != null && u.Email.Contains(searchString)) || 
                                       (u.Phone != null && u.Phone.Contains(searchString)));
            }

            Query = sortOrder switch
            {
                "name_desc" => Query.OrderByDescending(o => o.Name),
                "date" => Query.OrderBy(o => o.Date),
                "date_desc" => Query.OrderByDescending(o => o.Date),
                "status" => Query.OrderBy(o => o.Status),
                "status_desc" => Query.OrderByDescending(o => o.Status),
                _ => Query.OrderBy(o => o.Name),
            };

            int total = await Query.CountAsync();

            var order = await Query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            return View(order);
        }


        public IActionResult Create()
        {
            ViewBag.TourId = new SelectList(_context.Tour, "Id", "Name");
            ViewBag.UserId = new SelectList(_context.User, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {

            //User != null
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành công!";
                return RedirectToAction("Index");
            }
            //foreach (var item in ModelState)
            //{
            //    foreach (var error in item.Value.Errors)
            //    {
            //        Console.WriteLine($"[ModelState] {item.Key} => {error.ErrorMessage}");
            //    }
            //}
            return View(order);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.TourId = new SelectList(_context.Tour, "Id", "Name");
            ViewBag.UserId = new SelectList(_context.User, "Id", "Email");
            if (id == null) return NotFound();

            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Lấy order hiện tại từ database
                var existingOrder = await _context.Order.FindAsync(id);
                if (existingOrder == null) return NotFound();

                // Chỉ update các field cần thiết
                existingOrder.Name = order.Name;
                existingOrder.Phone = order.Phone;
                existingOrder.Address = order.Address;
                existingOrder.Status = order.Status;
                existingOrder.UserId = order.UserId;
                existingOrder.Date = order.Date;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Order.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            // Lấy order kèm theo User và Tour
            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Tour)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index");
            }

            // Lưu trạng thái cũ để so sánh
            var oldStatus = order.Status;
            order.Status = (byte?)status;
            
            try
            {
                await _context.SaveChangesAsync();
                
                // Thông báo dựa vào trạng thái mới
                string statusText = status switch
                {
                    0 => "Chờ xác nhận",
                    1 => "Đã xác nhận",
                    2 => "Đang giao",
                    3 => "Đã giao",
                    4 => "Đã hủy",
                    _ => "không xác định"
                };
                
                // Gửi email thông báo cho khách hàng
                if (!string.IsNullOrEmpty(order.Email) && oldStatus != status)
                {
                    try
                    {
                        string emailSubject = $"Cập nhật đơn hàng #{order.Id} - {statusText}";
                        string emailBody = GenerateOrderStatusEmailBody(order, statusText);
                        await _emailService.SendEmailAsync(order.Email, emailSubject, emailBody);
                    }
                    catch (Exception emailEx)
                    {
                        // Log lỗi email nhưng vẫn cho phép cập nhật thành công
                        Console.WriteLine($"Lỗi khi gửi email: {emailEx.Message}");
                    }
                }
                
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng thành '{statusText}' và gửi email thông báo!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái: " + ex.Message;
            }
            
            return RedirectToAction("Index");
        }

        private string GenerateOrderStatusEmailBody(Order order, string statusText)
        {
            string statusColor = order.Status switch
            {
                0 => "#ffc107", // Chờ xác nhận - vàng
                1 => "#17a2b8", // Đã xác nhận - xanh dương
                2 => "#007bff", // Đang giao - xanh biển
                3 => "#28a745", // Đã giao - xanh lá
                4 => "#dc3545", // Đã hủy - đỏ
                _ => "#6c757d"  // Mặc định - xám
            };

            string statusMessage = order.Status switch
            {
                0 => "Đơn hàng của bạn đang chờ xác nhận. Chúng tôi sẽ liên hệ với bạn sớm nhất.",
                1 => "Đơn hàng của bạn đã được xác nhận và đang được chuẩn bị.",
                2 => "Đơn hàng của bạn đang trên đường vận chuyển đến bạn.",
                3 => "Đơn hàng của bạn đã được giao thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!",
                4 => "Đơn hàng của bạn đã bị hủy. Nếu có thắc mắc, vui lòng liên hệ với chúng tôi.",
                _ => "Trạng thái đơn hàng đã được cập nhật."
            };

            string tourName = order.Tour?.Name ?? "N/A";
            string orderDate = order.Date?.ToString("dd/MM/yyyy") ?? "N/A";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #feb47b 0%, #ff7e5f 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .status-badge {{ display: inline-block; padding: 10px 20px; background: {statusColor}; color: white; border-radius: 5px; font-weight: bold; margin: 15px 0; }}
        .order-info {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }}
        .info-label {{ font-weight: bold; color: #666; }}
        .footer {{ text-align: center; margin-top: 30px; color: #999; font-size: 12px; }}
        .btn {{ display: inline-block; padding: 12px 30px; background: #ff7e5f; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎫 Cập Nhật Đơn Hàng</h1>
            <p>Xin chào {order.Name}!</p>
        </div>
        <div class='content'>
            <p>{statusMessage}</p>
            
            <div class='status-badge'>
                📦 Trạng thái: {statusText}
            </div>
            
            <div class='order-info'>
                <h3 style='margin-top: 0; color: #ff7e5f;'>📋 Thông tin đơn hàng</h3>
                <div class='info-row'>
                    <span class='info-label'>Mã đơn hàng:</span>
                    <span>#{order.Id}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Tour:</span>
                    <span>{tourName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Ngày đặt:</span>
                    <span>{orderDate}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Tên khách hàng:</span>
                    <span>{order.Name}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Số điện thoại:</span>
                    <span>{order.Phone}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Địa chỉ:</span>
                    <span>{order.Address}</span>
                </div>
            </div>
            
            <p style='margin-top: 20px;'>
                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại hỗ trợ.
            </p>
            
            <div style='text-align: center;'>
                <a href='#' class='btn'>Xem Chi Tiết Đơn Hàng</a>
            </div>
        </div>
        <div class='footer'>
            <p>© 2025 Travel Booking. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _context.Order
                    .Include(o => o.OrderDetail)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần xóa!";
                    return RedirectToAction("Index");
                }

                // Xóa OrderDetail trước
                if (order.OrderDetail != null && order.OrderDetail.Any())
                {
                    _context.OrderDetail.RemoveRange(order.OrderDetail);
                }

                // Xóa Order
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Xử lý lỗi khóa ngoại
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    TempData["ErrorMessage"] = "Không thể xóa đơn hàng này vì đang được tham chiếu bởi dữ liệu khác. Vui lòng liên hệ quản trị viên!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đơn hàng. Vui lòng thử lại!";
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
