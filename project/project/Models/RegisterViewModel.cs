using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; }

        public string? Address { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        public string? Role { get; set; } = "USER";
    }
}
