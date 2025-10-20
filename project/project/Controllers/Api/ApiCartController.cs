using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models.DTO;
using project.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace project.Controllers.Api
{
        [Route("api/[controller]")]
        [ApiController]
        [EnableCors("AllowOrigin")]
    public class ApiCartController : ControllerBase
        {
            private readonly AppDbContext _context;

            public ApiCartController(AppDbContext context)
            {
                _context = context;
            }

            [Authorize(AuthenticationSchemes = "UserScheme")]
            [HttpGet]
            public async Task<IActionResult> GetCart()
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null) return Ok(new List<CartItem>());

                var cartItems = await _context.CartItem
                                    .Where(ci => ci.CartId == cart.Id)
                                    .Include(ci => ci.Tour)
                                    .ToListAsync();

                return Ok(cartItems);
            }

            [Authorize(AuthenticationSchemes = "UserScheme")]
            [HttpPost("add")]
            public async Task<IActionResult> AddToCart([FromBody] AddCartDto model)
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId.Value, CreatedDate = DateTime.Now };
                    _context.Cart.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItem = await _context.CartItem.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.TourId == model.TourId);
                if (cartItem != null)
                {
                    cartItem.Quantity += model.Quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        TourId = model.TourId,
                        Quantity = model.Quantity
                    };
                    _context.CartItem.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }

            [Authorize(AuthenticationSchemes = "UserScheme")]
            [HttpPut("{id:int}")]
            public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateCartDto model)
            {
                var cartItem = await _context.CartItem.FirstOrDefaultAsync(c => c.Id == id);
                if (cartItem == null) return NotFound();

                cartItem.Quantity = model.Quantity;
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            [Authorize(AuthenticationSchemes = "UserScheme")]
            [HttpDelete("{id:int}")]
            public async Task<IActionResult> DeleteCartItem(int id)
            {
                var cartItem = await _context.CartItem.FirstOrDefaultAsync(c => c.Id == id);
                if (cartItem == null) return NotFound();

                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            [Authorize(AuthenticationSchemes = "UserScheme")]
            [HttpPost("checkout")]
            public async Task<IActionResult> Checkout([FromBody] CheckoutDto model)
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null) return BadRequest(new { message = "Giỏ hàng trống" });

                var cartItems = await _context.CartItem
                                .Where(ci => ci.CartId == cart.Id)
                                .Include(ci => ci.Tour)
                                .ToListAsync();

                if (!cartItems.Any()) return BadRequest(new { message = "Giỏ hàng trống" });

                var newOrder = new Order
                {
                    UserId = userId.Value,
                    Name = model.Name,
                    Phone = model.Phone,
                    Address = model.Address,
                    Status = 0,
                    Date = DateTime.Now
                };
                _context.Order.Add(newOrder);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    _context.OrderDetail.Add(new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        TourId = item.TourId,
                        Quantity = item.Quantity
                    });
                }

                _context.CartItem.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, orderId = newOrder.Id });
            }

            private int? GetUserId()
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId)) return userId;
                return null;
            }
        }
    
}
