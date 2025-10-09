using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class Contact
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? PostedDate { get; set; } = DateTime.Now;
        public string? Title { get; set; }
        public string? Content { get; set; }

    }
} 
