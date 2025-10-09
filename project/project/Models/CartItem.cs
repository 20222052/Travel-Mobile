using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace project.Models
{
    public class CartItem
    {
        public int? Id { get; set; }

        public int? CartId { get; set; }

        public int? TourId { get; set; }
        
        public int? Quantity { get; set; }

        // Navigation 
        public virtual Cart? Cart { get; set; }

        public virtual Tour? Tour { get; set; }

    }
}
