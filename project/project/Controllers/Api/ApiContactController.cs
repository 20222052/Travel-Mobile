using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ApiContactController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                _context.Contact.Add(contact);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã gửi yêu cầu của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }
    }
}
