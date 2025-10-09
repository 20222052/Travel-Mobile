using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class OrderDetail
    {
        public int? Id { get; set; }
        public int? TourId { get; set; }
        public int? OrderId { get; set; }
        public int? Quantity { get; set; }
        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual Tour? Tour { get; set; }

    }
} 
