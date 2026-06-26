using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Customer
{
    public class UpdateCustomerRequest
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc.")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Giới tính không hợp lệ.")]
        public string Gender { get; set; } = "Other";

        [Required(ErrorMessage = "Trình độ là bắt buộc.")]
        [RegularExpression("^(Beginner|Intermediate|Advanced)$", ErrorMessage = "Trình độ không hợp lệ.")]
        public string SkillLevel { get; set; } = "Beginner";

        [Range(0, 1000000, ErrorMessage = "Điểm tích lũy không hợp lệ.")]
        public int LoyaltyPoints { get; set; }

        public int? MembershipTierId { get; set; }

        public bool IsActive { get; set; }
    }
}
