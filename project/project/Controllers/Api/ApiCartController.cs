using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.Services;
using System.Security.Claims;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class ApiCartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrderEmailService _orderEmailService;
        private readonly ILogger<ApiCartController> _logger;

        public ApiCartController(
            AppDbContext context,
            IOrderEmailService orderEmailService,
            ILogger<ApiCartController> logger)
        {
            _context = context;
            _orderEmailService = orderEmailService;
            _logger = logger;
        }

        // GET: api/ApiCart
        // Lấy giỏ hàng của user hiện tại
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return Ok(new List<CartItem>()); // Trả về mảng rỗng nếu chưa có cart
            }

            var cartItems = await _context.CartItem
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Tour)
                .ToListAsync();

            return Ok(cartItems);
        }

        // POST: api/ApiCart/add
        // Thêm tour vào giỏ hàng
        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            try
            {
                // Tìm hoặc tạo cart cho user
                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Cart.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Kiểm tra tour có tồn tại không
                var tour = await _context.Tour.FindAsync(request.TourId);
                if (tour == null)
                {
                    return NotFound(new { message = "Tour không tồn tại" });
                }

                // Kiểm tra tour đã có trong giỏ chưa
                var existingItem = await _context.CartItem
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.TourId == request.TourId);

                if (existingItem != null)
                {
                    // Nếu đã có thì tăng số lượng
                    existingItem.Quantity += request.Quantity;
                    _context.Update(existingItem);
                }
                else
                {
                    // Nếu chưa có thì thêm mới
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        TourId = request.TourId,
                        Quantity = request.Quantity
                    };
                    _context.CartItem.Add(newItem);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        // PUT: api/ApiCart/{id}
        // Cập nhật số lượng trong giỏ
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItem
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ" });
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(new { message = "Số lượng phải lớn hơn 0" });
            }

            cartItem.Quantity = request.Quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật số lượng" });
        }

        // DELETE: api/ApiCart/{id}
        // Xóa item khỏi giỏ hàng
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItem
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ" });
            }

            _context.CartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa khỏi giỏ hàng" });
        }

        // POST: api/ApiCart/checkout
        // Đặt hàng từ giỏ hàng
        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            try
            {
                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    return BadRequest(new { message = "Giỏ hàng trống" });
                }

                var cartItems = await _context.CartItem
                    .Where(ci => ci.CartId == cart.Id)
                    .Include(ci => ci.Tour)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return BadRequest(new { message = "Giỏ hàng trống" });
                }

                // Lấy thông tin user
                var user = await _context.User.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Tạo một đơn hàng duy nhất (lấy tour đầu tiên làm TourId chính)
                var firstTour = cartItems.First();
                var order = new Order
                {
                    UserId = userId,
                    TourId = firstTour.TourId, // Tour đầu tiên
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = user.Email,
                    Gender = user.Gender,
                    Address = request.Address,
                    Status = 0, // Chờ xác nhận
                    Date = DateTime.Now
                };

                _context.Order.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {order.Id} created for user {userId}");

                // Tạo OrderDetail cho từng tour trong giỏ hàng
                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        TourId = item.TourId,
                        Quantity = item.Quantity // ✅ Lưu số lượng
                    };

                    _context.OrderDetail.Add(orderDetail);
                    _logger.LogInformation($"OrderDetail created for Order {order.Id}, Tour {item.TourId}, Quantity {item.Quantity}");
                }

                await _context.SaveChangesAsync();

                // Gửi email thông báo
                try
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        // Lấy lại order với đầy đủ thông tin OrderDetail và Tour
                        var orderWithDetails = await _context.Order
                            .Include(o => o.Tour)
                            .FirstOrDefaultAsync(o => o.Id == order.Id);

                        // Lấy danh sách OrderDetail với Tour
                        var orderDetailsList = await _context.OrderDetail
                            .Where(od => od.OrderId == order.Id)
                            .Include(od => od.Tour)
                            .ToListAsync();

                        if (orderWithDetails != null)
                        {
                            await _orderEmailService.SendOrderCreatedEmailAsync(
                                orderWithDetails,
                                user,
                                orderWithDetails.Tour,
                                orderDetailsList
                            );
                            _logger.LogInformation($"Order creation email sent to {user.Email} for Order #{order.Id} with {orderDetailsList.Count} items");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Cannot send email: User email is empty for Order #{order.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send order creation email for Order #{order.Id}");
                    // Không throw để không ảnh hưởng đến việc tạo đơn hàng
                }

                // Xóa giỏ hàng sau khi đặt xong
                _context.CartItem.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checkout completed for user {userId}, Order #{order.Id} with {cartItems.Count} items");

                return Ok(new
                {
                    message = "Đặt hàng thành công",
                    orderId = order.Id,
                    itemCount = cartItems.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi đặt hàng", error = ex.Message });
            }
        }
    }

    // Request models
    public class AddToCartRequest
    {
        public int TourId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateQuantityRequest
    {
        public int Quantity { get; set; }
    }

    public class CheckoutRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
