using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class Order
    {
        public int? Id { get; set; }

        public int? TourId { get; set; }

        public string? Name { get; set; }

        public int? UserId { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; }

        public DateTime? Date { get; set; } = DateTime.Now;

        // Status:
        // 0: Chờ xác nhận
        // 1: Đã xác nhận
        // 2: Đang giao
        // 3: Đã giao
        // 4: Đã hủy
        public byte? Status { get; set; } = 0;

        // Navigation Properties
        public virtual Tour? Tour { get; set; }
        public virtual User? User { get; set; }

        // Helper method to get status text
        public string GetStatusText()
        {
            return Status switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đang giao",
                3 => "Đã giao",
                4 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        // Helper method to get status badge class
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                0 => "badge-warning",      // Vàng - Chờ xác nhận
                1 => "badge-info",          // Xanh dương - Đã xác nhận
                2 => "badge-primary",       // Xanh đậm - Đang giao
                3 => "badge-success",       // Xanh lá - Đã giao
                4 => "badge-danger",        // Đỏ - Đã hủy
                _ => "badge-secondary"
            };
        }
    }
} 
