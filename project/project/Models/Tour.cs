using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace project.Models
{
    public class Tour
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Image { get; set; }

        public string? Location { get; set; }

        public string? Duration { get; set; }

        public double? Price { get; set; }

        public int? People { get; set; }
        public int? View { get; set; } = 1;

        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; }

        // Navigation Property
        public virtual Category? Category { get; set; }

        // Navigation Properties
        public virtual ICollection<History>? Histories { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; } // dùng để upload
    }
}
