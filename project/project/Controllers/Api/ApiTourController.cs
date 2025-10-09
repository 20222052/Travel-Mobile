using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetTours([FromQuery] string? name)
        {
            if (_context.Tour == null)
            {
                return NotFound();
            }
            var tours = from p in _context.Tour
                           select p;
            if (!string.IsNullOrWhiteSpace(name))
            {
                tours = from p in _context.Tour
                           where p.Name.Contains(name)
                           select p;
            }
            return await tours.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetTour(string id)
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

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
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

        private bool TourExists(int? id)
        {
            return (_context.Tour?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
