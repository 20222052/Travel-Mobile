using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class Order
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public int? UserId { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public DateTime? Date { get; set; } = DateTime.Now;

        public byte? Status { get; set; } = 0;

        // Navigation Properties
        public virtual User? User { get; set; }

    }
} 
