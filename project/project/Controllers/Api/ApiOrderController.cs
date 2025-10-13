using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.Services;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class ApiOrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrderEmailService _orderEmailService;
        private readonly ILogger<ApiOrderController> _logger;

        public ApiOrderController(
            AppDbContext context,
            IOrderEmailService orderEmailService,
            ILogger<ApiOrderController> logger)
        {
            _context = context;
            _orderEmailService = orderEmailService;
            _logger = logger;
        }

        // GET: api/ApiOrder?id={userId}&search=&sort=desc
        // Lấy danh sách đơn hàng của user hiện tại
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
            [FromQuery] int id, 
            [FromQuery] string? search = null, 
            [FromQuery] string sort = "desc")
        {
            if (_context.Order == null)
            {
                return NotFound();
            }

            var query = _context.Order
                .Include(o => o.Tour)
                .Where(o => o.UserId == id);

            // Tìm kiếm nếu có
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => 
                    (o.Tour != null && o.Tour.Name != null && o.Tour.Name.Contains(search)) ||
                    (o.Name != null && o.Name.Contains(search)) ||
                    (o.Phone != null && o.Phone.Contains(search))
                );
            }

            // Sắp xếp
            if (sort.ToLower() == "asc")
            {
                query = query.OrderBy(o => o.Date);
            }
            else
            {
                query = query.OrderByDescending(o => o.Date);
            }

            var orders = await query.ToListAsync();

            return orders;
        }
        
        // GET: api/ApiOrder/my-orders?userId={userId}
        // Endpoint cũ để tương thích ngược
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders([FromQuery] int userId)
        {
            return await GetOrders(userId, null, "desc");
        }

        // GET: api/ApiOrder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            if (_context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Tour)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/ApiOrder
        // Tạo đơn hàng mới từ mobile app
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            if (_context.Order == null)
            {
                return Problem("Entity set 'AppDbContext.Order' is null.");
            }

            try
            {
                // Set default values
                order.Date = DateTime.Now;
                order.Status = 0; // Chờ xác nhận

                _context.Order.Add(order);
                await _context.SaveChangesAsync();

                // Gửi email thông báo đơn hàng mới
                try
                {
                    var user = await _context.User.FindAsync(order.UserId);
                    var tour = await _context.Tour.FindAsync(order.TourId);
                    
                    if (user != null)
                    {
                        await _orderEmailService.SendOrderCreatedEmailAsync(order, user, tour);
                        _logger.LogInformation($"Order created email sent to {user.Email} for Order #{order.Id}");
                    }
                    else
                    {
                        _logger.LogWarning($"User not found for Order #{order.Id}, UserId: {order.UserId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send order creation email for Order #{order.Id}");
                    // Không throw để không ảnh hưởng đến việc tạo đơn hàng
                }

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo đơn hàng", error = ex.Message });
            }
        }

        // PUT: api/ApiOrder/cancel/5
        // Hủy đơn hàng (chỉ cho phép hủy khi status = 0 hoặc 1)
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] int userId)
        {
            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Tour)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép hủy đơn hàng ở trạng thái Chờ xác nhận hoặc Đã xác nhận
            if (order.Status != 0 && order.Status != 1)
            {
                return BadRequest(new { message = "Không thể hủy đơn hàng ở trạng thái này" });
            }

            var oldStatus = order.Status ?? 0;
            order.Status = 4; // Đã hủy
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Gửi email thông báo
            try
            {
                if (order.User != null)
                {
                    await _orderEmailService.SendOrderStatusChangedEmailAsync(order, order.User, order.Tour, oldStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation email");
            }

            return Ok(new { message = "Đã hủy đơn hàng thành công" });
        }

        // GET: api/ApiOrder/orderDetail/{orderId}
        // Lấy chi tiết đơn hàng
        [HttpGet("orderDetail/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails(int orderId)
        {
            if (_context.OrderDetail == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetail
                .Include(od => od.Tour)
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            return orderDetails;
        }

        // GET: api/ApiOrder/status-list
        // Lấy danh sách các trạng thái
        [HttpGet("status-list")]
        public ActionResult<object> GetStatusList()
        {
            return Ok(new
            {
                statuses = new[]
                {
                    new { value = 0, text = "Chờ xác nhận", color = "#ffc107" },
                    new { value = 1, text = "Đã xác nhận", color = "#17a2b8" },
                    new { value = 2, text = "Đang giao", color = "#007bff" },
                    new { value = 3, text = "Đã giao", color = "#28a745" },
                    new { value = 4, text = "Đã hủy", color = "#dc3545" }
                }
            });
        }
    }
}
