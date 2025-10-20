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

        public ApiBlogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ApiBlog
        [HttpGet]
        public async Task<IActionResult> GetBlogs(int page = 1, int pageSize = 6, int? categoryId = null, string? title = "", string? sort = "desc")
        {
            IQueryable<Blog> query = _context.Blog;

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.Title.Contains(title));
            }

            if (categoryId != null)
            {
                query = query.Where(b => b.CategoryId == categoryId);
            }

            if (sort == "desc")
            {
                query = query.OrderByDescending(b => b.Id);
            }
            else if (sort == "asc")
            {
                query = query.OrderBy(b => b.Id);
            }

            var blogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                items = blogs,
                totalPages = totalPages,
                catId = categoryId
            });
        }

        // GET: api/ApiBlog/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            var blog = await _context.Blog.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        // PUT: api/ApiBlog/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutBlog(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                return BadRequest();
            }

            _context.Entry(blog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ApiBlog
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog(Blog blog)
        {
            _context.Blog.Add(blog);
            await _context.SaveChangesAsync();

            return Ok(blog);
        }

        // DELETE: api/ApiBlog/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blog.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blog.Remove(blog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return _context.Blog.Any(e => e.Id == id);
        }
    }
}
