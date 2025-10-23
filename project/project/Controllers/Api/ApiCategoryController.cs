using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ApiCategoryController(AppDbContext context) => _context = context;

        // =======================
        // DIAGNOSTIC (tạm thời)
        // GET: api/ApiCategory/diag
        // Dùng để kiểm tra API đang kết nối DB nào
        // =======================
        [HttpGet("diag")]
        public IActionResult Diag([FromServices] AppDbContext ctx)
        {
            var cnn = ctx.Database.GetDbConnection();
            return Ok(new
            {
                DataSource = cnn.DataSource,   // server/instance
                Database = cnn.Database,     // tên DB thực tế
                Connection = cnn.ConnectionString
            });
        }

        // =======================
        // GET: api/ApiCategory?page=1&pageSize=10&keyword=&sort=desc
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetCategories(
            int page = 1, int pageSize = 10,
            string? keyword = "", string sort = "desc")
        {
            IQueryable<Category> q = _context.Category.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(c => c.Type!.Contains(keyword));

            q = sort == "asc" ? q.OrderBy(c => c.Id) : q.OrderByDescending(c => c.Id);

            var totalCount = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new { items, totalPages, totalCount, page, pageSize });
        }

        // =======================
        // GET: api/ApiCategory/5
        // =======================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var cat = await _context.Category.AsNoTracking()
                                             .FirstOrDefaultAsync(c => c.Id == id);
            return cat is null ? NotFound() : cat;
        }

        // =======================
        // POST: api/ApiCategory
        // Body JSON: { "type": "Tên danh mục" }
        // =======================
        [HttpPost]
        public async Task<IActionResult> PostCategory([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Type))
                return BadRequest(new { message = "Type is required." });

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // =======================
        // PUT: api/ApiCategory/5
        // Body JSON: { "type": "Tên mới" }
        // =======================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] Category category)
        {
            var existing = await _context.Category.FindAsync(id);
            if (existing is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(category.Type))
                existing.Type = category.Type;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =======================
        // DELETE: api/ApiCategory/5
        // =======================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Category.FindAsync(id);
            if (cat is null) return NotFound();

            _context.Category.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
