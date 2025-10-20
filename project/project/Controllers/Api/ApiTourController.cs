using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using static NuGet.Packaging.PackagingConstants;

namespace project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class ApiTourController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiTourController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetTours(int page = 1, int pageSize = 6, int? categoryId = null, string? name = "", string? sort = "desc")
        {
            IQueryable<Tour> query = _context.Tour;
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.Contains(name));
            }
            if(categoryId != null )
            {
                query = query.Where(t => t.CategoryId == categoryId);
            }    
            if (sort == "desc")
            {
                query = query.OrderByDescending(t => t.Id);
            }
            else if (sort == "asc")
            {
                query = query.OrderBy(t => t.Id);
            } 


            var tours = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Đếm tổng số tour sau khi filter
            int totalCount = await query.CountAsync();
            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Trả về JSON gồm danh sách + tổng số trang
            return Ok(new
            {
                items = tours,
                totalPages = totalPages,
                catId = categoryId
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tour>> GetTour(int id)
        {
            if (_context.Tour == null)
            {
                return NotFound();
            }
            var tour = await _context.Tour.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            return tour;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProduct(int id, Tour tour)
        {
            if (id != tour.Id)
            {
                return BadRequest();
            }

            _context.Entry(tour).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Tour>> PostProduct(Tour tour)
        {
            if (_context.Tour == null)
            {
                return Problem("Entity set 'C2404LMDbContext.Products'  is null.");
            }
            _context.Tour.Add(tour);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TourExists(tour.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(tour);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Tour == null)
            {
                return NotFound();
            }
            var tour = await _context.Tour.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            _context.Tour.Remove(tour);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomTours()
        {
            var randomTours = await _context.Tour
                .OrderBy(t => Guid.NewGuid())
                .Include(t => t.Category)
                .Take(4)
                .ToListAsync();
            return Ok(randomTours);
        }

        [HttpGet("topviewed")]
        public async Task<IActionResult> GetTopViewedTours()
        {
            var topViewedTours = await _context.Tour
                .OrderByDescending(t => t.View)
                .Include(t => t.Category)
                .Take(3)
                .ToListAsync();
            return Ok(topViewedTours);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestTour()
        {
            var latestTour = await _context.Tour
                .OrderByDescending(t => t.CreatedDate)
                .Include(t => t.Category)
                .FirstOrDefaultAsync();
            return Ok(latestTour);
        }

        [HttpGet("hotel")]
        public async Task<IActionResult> GetHotelTours()
        {
            var hotelTours = await _context.Tour
                .Include(t => t.Category)
                .Where(t => t.CategoryId == 5)
                .OrderByDescending(t => t.CreatedDate)
                .Take(3)
                .ToListAsync();
            return Ok(hotelTours);
        }
        private bool TourExists(int? id)
        {
            return (_context.Tour?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
