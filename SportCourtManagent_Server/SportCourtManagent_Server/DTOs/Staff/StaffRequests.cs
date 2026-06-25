using SportCourtManagent_Server.Enums;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Staff
{
    public class CreateShiftRequest
    {
        [Required(ErrorMessage = "StaffId không được để trống.")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Ngày trực không được để trống.")]
        public DateOnly ShiftDate { get; set; }

        [Required(ErrorMessage = "Loại ca không được để trống.")]
        public ShiftType ShiftType { get; set; }

        [MaxLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự.")]
        public string? Note { get; set; }
    }

    public class UpdateShiftRequest
    {
        [Required(ErrorMessage = "Loại ca không được để trống.")]
        public ShiftType ShiftType { get; set; }

        [MaxLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự.")]
        public string? Note { get; set; }
    }

    public class BulkCreateShiftRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 ca trong danh sách.")]
        public List<CreateShiftRequest> Shifts { get; set; } = new();
    }
}
