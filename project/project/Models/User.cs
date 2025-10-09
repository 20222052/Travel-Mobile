using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? Name { get; set; }
        public string? Image { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string? Gender { get; set; }

        public string? Address { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; } = null!;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Vui long chọn quyền")]
        public string? Role { get; set; } = "user";

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<History>? Histories { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; } // dùng để upload
    }
}
