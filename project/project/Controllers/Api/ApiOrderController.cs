using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiOrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiOrderController(AppDbContext context)
        {
            _context = context;
        }

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
    }
}
