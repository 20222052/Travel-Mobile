using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiBlogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ApiBlogController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/ApiBlog
        [HttpGet]
        public async Task<IActionResult> GetBlogs(
            int page = 1, int pageSize = 10,
            int? categoryId = null, string? title = "", string? sort = "desc")
        {
            IQueryable<Blog> query = _context.Blog.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title!.Contains(title));

            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId);

            query = sort == "asc" ? query.OrderBy(b => b.Id) : query.OrderByDescending(b => b.Id);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new { items, totalPages, totalCount, page, pageSize });
        }

        // GET: api/ApiBlog/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            var blog = await _context.Blog.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return blog is null ? NotFound() : blog;
        }

        // POST: api/ApiBlog  (multipart/form-data)
        [HttpPost]
        [RequestSizeLimit(20_000_000)] // 20MB
        public async Task<IActionResult> PostBlog([FromForm] Blog blog)
        {
            if (blog.ImageFile is not null && blog.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(blog.ImageFile.FileName)}";
                var fullPath = Path.Combine(uploads, fileName);
                using var stream = System.IO.File.Create(fullPath);
                await blog.ImageFile.CopyToAsync(stream);

                blog.Image = $"/uploads/{fileName}";
            }

            blog.View ??= 0;
            blog.PostedDate ??= DateTime.Now;

            _context.Blog.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
        }

        // PUT: api/ApiBlog/5 (multipart/form-data)
        [HttpPut("{id:int}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> PutBlog(int id, [FromForm] Blog blog)
        {
            var existing = await _context.Blog.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Title = blog.Title;
            existing.Content = blog.Content;
            existing.CategoryId = blog.CategoryId;
            existing.View = blog.View ?? existing.View;
            existing.PostedDate = blog.PostedDate ?? existing.PostedDate;

            if (blog.ImageFile is not null && blog.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(blog.ImageFile.FileName)}";
                var fullPath = Path.Combine(uploads, fileName);
                using var stream = System.IO.File.Create(fullPath);
                await blog.ImageFile.CopyToAsync(stream);

                existing.Image = $"/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/ApiBlog/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blog.FindAsync(id);
            if (blog is null) return NotFound();

            _context.Blog.Remove(blog);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
