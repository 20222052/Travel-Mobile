using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class History
    {
        public int? Id { get; set; }

        public int? UserId { get; set; }

        public int? TourId { get; set; }

        public string? Content { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public byte? Status { get; set; } = 0;

        // Navigation Properties
        public virtual User? User { get; set; }

        public virtual Tour? Tour { get; set; }
    }
}
