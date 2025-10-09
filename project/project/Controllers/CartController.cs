using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Security.Claims;

namespace project.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Order()
        {
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = int.Parse(user);
            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItems = await _context.CartItem
                            .Where(ci => ci.CartId == cart.Id)
                            .Include(ci => ci.Tour)
                            .ToListAsync();
            //ViewBag.cartItems = cartItems;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> CartUpdate(int id, int quantity)
        {

            var cartItem = _context.CartItem.FirstOrDefault(c => c.Id == id);
            if (cartItem == null)
            {
                return NotFound();
            }
            cartItem.Quantity = quantity;
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public async Task<IActionResult> OrderInfo()
        {
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(user);
            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItems = await _context.CartItem
                            .Where(ci => ci.CartId == cart.Id)
                            .Include(ci => ci.Tour)
                            .ToListAsync();
            //ViewBag.cartItems = cartItems;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> OrderInfo(string name, string phone, string address)
        {
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(user);
            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItems = await _context.CartItem
                            .Where(ci => ci.CartId == cart.Id)
                            .Include(ci => ci.Tour)
                            .ToListAsync();
            //ViewBag.cartItems = cartItems;
            if (cartItems == null || !cartItems.Any())
            {
                // Giỏ hàng trống
                return RedirectToAction("Order", "Home");
            }

            //Tạo đơn hàng mới
            var newOrder = new Order
            {
                UserId = userId,
                Name = name,
                Phone = phone,
                Address = address,
                Status = 0, 
                Date = DateTime.Now
            };
            _context.Order.Add(newOrder);
            await _context.SaveChangesAsync();
            // Lấy id đơn hàng vừa tạo
            int? orderId = newOrder.Id;

            // Thêm các sản phẩm vào OrderDetail
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    TourId = item.TourId,
                    Quantity = item.Quantity
                };

                _context.OrderDetail.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            //Xóa giỏ hàng sau khi đặt xong
            _context.CartItem.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("History","Account");
        }
        public async Task<IActionResult> AddCart(int id)
        {
            int quantity = 1;
            // Lấy thông tin user đăng nhập
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(user);

            // Kiểm tra xem user đã có Cart chưa
            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra xem sản phẩm đã có trong CartItem chưa
            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.TourId == id);

            if (cartItem != null)
            {
                // Nếu có thì cộng thêm số lượng
                cartItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có thì thêm mới
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    TourId = id,
                    Quantity = quantity
                };
                _context.CartItem.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Lấy url trang trước
            var referer = Request.Headers["Referer"].ToString();

            // Kiểm tra nếu có referer thì quay về, nếu không có thì về trang chủ
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


    }
}
