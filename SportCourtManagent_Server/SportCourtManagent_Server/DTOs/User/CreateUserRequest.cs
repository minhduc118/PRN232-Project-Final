using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$", ErrorMessage = "Mật khẩu phải dài tối thiểu 8 ký tự, chứa ít nhất 1 chữ cái in hoa và 1 ký tự đặc biệt.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public string Role { get; set; } = string.Empty;

        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Giới tính phải là Male, Female hoặc Other.")]
        public string Gender { get; set; } = "Other";

        public string SkillLevel { get; set; } = "Beginner";

        public bool IsActive { get; set; } = true;
    }
}
