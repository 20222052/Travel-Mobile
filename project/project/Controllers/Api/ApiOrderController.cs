using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.Services;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiOrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ApiOrderController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ============ CÁC API ĐÃ CÓ (GIỮ NGUYÊN) ============

        // GET: /api/ApiOrder?id=1&search=&sort=desc
        [HttpGet]
        public async Task<IActionResult> GetOrderUserId(int id, string? search = "", string? sort = "desc")
        {
            IQueryable<Order> query = _context.Order.Where(b => b.UserId == id);
                
            if (sort == "desc")
            {
                query = query.OrderByDescending(b => b.Id);
            }
            else if (sort == "asc")
            {
                query = query.OrderBy(b => b.Id);
            }

            if (search != null)
            {
                query = query.Where(b => b.Address.Contains(search) || b.Phone.Contains(search) || b.Name.Contains(search));
            }

            var orders = await query
                .ToListAsync();

            return Ok(orders);
        }

        // GET: /api/ApiOrder/orderDetail/1
        [HttpGet("orderDetail/{id:int}")]
        public IActionResult GetOrderDetailByOrderId(int id)
        {
            var orderDetail = _context.OrderDetail.Where(b => b.OrderId == id).ToList();
            if (orderDetail == null)
            {
                return NotFound();
            }
            return Ok(orderDetail);
        }

        // ============ CÁC API MỚI (COPY TỪ OrderController) ============

        // GET: /api/ApiOrder/all?searchString=&sortOrder=&page=1&pageSize=6
        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN,EDITOR")]
        public async Task<IActionResult> GetAllOrders(string? searchString, string? sortOrder, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Order
                    .Include(o => o.User)
                    .Include(o => o.Tour)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(u => (u.Name != null && u.Name.Contains(searchString)) || 
                                           (u.Email != null && u.Email.Contains(searchString)) || 
                                           (u.Phone != null && u.Phone.Contains(searchString)) ||
                                           (u.Address != null && u.Address.Contains(searchString)));
                }

                query = sortOrder switch
                {
                    "name_asc" => query.OrderBy(o => o.Name),
                    "name_desc" => query.OrderByDescending(o => o.Name),
                    "date_asc" => query.OrderBy(o => o.Date),
                    "date_desc" => query.OrderByDescending(o => o.Date),
                    "status" => query.OrderBy(o => o.Status),
                    "status_desc" => query.OrderByDescending(o => o.Status),
                    _ => query.OrderByDescending(o => o.Date), // Default: ngày mới nhất
                };

                int total = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(total / (double)pageSize);

                var orders = await query
                    .Include(o => o.OrderDetail)
                        .ThenInclude(od => od.Tour)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = orders.Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Email,
                    o.Phone,
                    o.Address,
                    o.Date,
                    o.Status,
                    StatusName = GetStatusName(o.Status),
                    Gender = "Nam", // Default gender
                    TotalPrice = o.OrderDetail?.Sum(od => (od.Quantity ?? 0) * (od.Tour?.Price ?? 0)) ?? 0,
                    o.UserId,
                    o.TourId,
                    TourName = o.Tour != null ? o.Tour.Name : null,
                    TourImage = o.Tour != null ? o.Tour.Image : null,
                    TourPrice = o.Tour != null ? o.Tour.Price : null,
                    UserName = o.User != null ? o.User.Name : null,
                    UserEmail = o.User != null ? o.User.Email : null,
                    OrderDetailCount = o.OrderDetail?.Count ?? 0
                }).ToList();

                return Ok(new
                {
                    Orders = result,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalRecords = total,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng.", error = ex.Message });
            }
        }

        // GET: /api/ApiOrder/detail/1
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _context.Order
                    .Include(o => o.User)
                    .Include(o => o.Tour)
                    .Include(o => o.OrderDetail)
                        .ThenInclude(od => od.Tour)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                var totalPrice = order.OrderDetail?.Sum(od => (od.Quantity ?? 0) * (od.Tour?.Price ?? 0)) ?? 0;

                var result = new
                {
                    order.Id,
                    order.Name,
                    order.Email,
                    order.Phone,
                    order.Address,
                    order.Date,
                    order.Status,
                    Gender = "Nam", // Default gender
                    StatusName = GetStatusName(order.Status),
                    TotalPrice = totalPrice,
                    order.UserId,
                    order.TourId,
                    TourName = order.Tour?.Name,
                    TourImage = order.Tour?.Image,
                    TourPrice = order.Tour?.Price,
                    TourDescription = order.Tour?.Description,
                    UserName = order.User?.Name,
                    UserEmail = order.User?.Email,
                    UserPhone = order.User?.Phone,
                    OrderDetails = order.OrderDetail?.Select(od => new
                    {
                        od.Id,
                        od.OrderId,
                        od.TourId,
                        od.Quantity,
                        TourName = od.Tour?.Name,
                        TourImage = od.Tour?.Image,
                        TourPrice = od.Tour?.Price,
                        Subtotal = (od.Quantity ?? 0) * (od.Tour?.Price ?? 0)
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin đơn hàng.", error = ex.Message });
            }
        }

        // POST: /api/ApiOrder/create
        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN,EDITOR")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Add(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm đơn hàng thành công!", orderId = order.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo đơn hàng.", error = ex.Message });
            }
        }

        // PUT: /api/ApiOrder/update/1
        [HttpPut("update/{id:int}")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN,EDITOR")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            try
            {
                if (id != order.Id)
                {
                    return BadRequest(new { message = "ID không khớp." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var existingOrder = await _context.Order.FindAsync(id);
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                // Cập nhật các field (không có TotalPrice vì tính từ OrderDetail)
                existingOrder.Name = order.Name;
                existingOrder.Phone = order.Phone;
                existingOrder.Address = order.Address;
                existingOrder.Email = order.Email;
                existingOrder.Status = order.Status;
                existingOrder.UserId = order.UserId;
                existingOrder.TourId = order.TourId;
                existingOrder.Date = order.Date;

                try
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Cập nhật đơn hàng thành công!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Order.Any(e => e.Id == id))
                        return NotFound(new { message = "Đơn hàng không tồn tại." });
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật đơn hàng.", error = ex.Message });
            }
        }

        // PUT: /api/ApiOrder/update-status/1
        [HttpPut("update-status/{id:int}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var order = await _context.Order
                    .Include(o => o.User)
                    .Include(o => o.Tour)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                var oldStatus = order.Status;
                order.Status = (byte?)request.Status;

                await _context.SaveChangesAsync();

                string statusText = GetStatusName(request.Status);

                // Gửi email thông báo nếu có email và trạng thái thay đổi
                if (!string.IsNullOrEmpty(order.Email) && oldStatus != request.Status)
                {
                    try
                    {
                        string emailSubject = $"Cập nhật đơn hàng #{order.Id} - {statusText}";
                        string emailBody = GenerateOrderStatusEmailBody(order, statusText);
                        await _emailService.SendEmailAsync(order.Email, emailSubject, emailBody);
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Lỗi khi gửi email: {emailEx.Message}");
                    }
                }

                return Ok(new { 
                    message = $"Đã cập nhật trạng thái đơn hàng thành '{statusText}' và gửi email thông báo!",
                    status = request.Status,
                    statusName = statusText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái đơn hàng.", error = ex.Message });
            }
        }

        // DELETE: /api/ApiOrder/delete/1
        [HttpDelete("delete/{id:int}")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Order
                    .Include(o => o.OrderDetail)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng cần xóa." });
                }

                // Xóa OrderDetail trước
                if (order.OrderDetail != null && order.OrderDetail.Any())
                {
                    _context.OrderDetail.RemoveRange(order.OrderDetail);
                }

                // Xóa Order
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa đơn hàng thành công!" });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("FOREIGN KEY") || 
                     ex.InnerException.Message.Contains("REFERENCE constraint")))
                {
                    return BadRequest(new { message = "Không thể xóa đơn hàng này vì đang được tham chiếu bởi dữ liệu khác." });
                }
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa đơn hàng.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa đơn hàng.", error = ex.Message });
            }
        }

        // GET: /api/ApiOrder/statistics
        [HttpGet("statistics")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "ADMIN,EDITOR")]
        public async Task<IActionResult> GetOrderStatistics()
        {
            try
            {
                var total = await _context.Order.CountAsync();
                var pending = await _context.Order.Where(o => o.Status == 0).CountAsync();
                var confirmed = await _context.Order.Where(o => o.Status == 1).CountAsync();
                var processing = await _context.Order.Where(o => o.Status == 2).CountAsync();
                var completed = await _context.Order.Where(o => o.Status == 3).CountAsync();
                var cancelled = await _context.Order.Where(o => o.Status == 4).CountAsync();

                return Ok(new
                {
                    Total = total,
                    Pending = pending,
                    Confirmed = confirmed,
                    Processing = processing,
                    Completed = completed,
                    Cancelled = cancelled
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê đơn hàng.", error = ex.Message });
            }
        }

        // ============ HELPER METHODS ============

        private string GetStatusName(int? status)
        {
            return status switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đang giao",
                3 => "Đã giao",
                4 => "Đã hủy",
                _ => "Không xác định"
            };
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
    }

    // DTO for update status request
    public class UpdateStatusRequest
    {
        public int Status { get; set; }
    }
}
