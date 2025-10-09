using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace project.Models
{
    public class Cart
    {
        public int? Id { get; set; }

        public int? UserId { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
     
        // Navigation Property
        public virtual User? User { get; set; }

    }
}
