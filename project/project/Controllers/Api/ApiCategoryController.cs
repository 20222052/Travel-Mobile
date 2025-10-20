using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiCategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var categories = await _context.Category.ToListAsync();
            return Ok(categories);
        }
    }
}
