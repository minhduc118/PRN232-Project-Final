using SportCourtManagent_Server.Enums;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Maintenance
{
    public class CreateMaintenanceRequest
    {
        [Required(ErrorMessage = "CourtId không được để trống.")]
        public int? CourtId { get; set; }

        [Required(ErrorMessage = "Loại bảo trì không được để trống.")]
        public MaintenanceType? MaintenanceType { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        public DateTime? StartDateTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        public DateTime? EndDateTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên phụ trách.")]
        public int? AssignedStaffId { get; set; }

        [Required(ErrorMessage = "Lý do bảo trì không được để trống.")]
        [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
    }

    public class UpdateMaintenanceRequest
    {
        [Required(ErrorMessage = "CourtId không được để trống.")]
        public int? CourtId { get; set; }

        [Required(ErrorMessage = "Loại bảo trì không được để trống.")]
        public MaintenanceType? MaintenanceType { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        public DateTime? StartDateTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        public DateTime? EndDateTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên phụ trách.")]
        public int? AssignedStaffId { get; set; }

        [Required(ErrorMessage = "Lý do bảo trì không được để trống.")]
        [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public MaintenanceStatus? Status { get; set; }

        [MaxLength(500, ErrorMessage = "Kết quả ghi chú không được vượt quá 500 ký tự.")]
        public string? Result { get; set; }

        [MaxLength(500, ErrorMessage = "Đường dẫn ảnh minh chứng không được vượt quá 500 ký tự.")]
        public string? ImageProof { get; set; }
    }


    public class VerifyMaintenanceRequest
    {
        [Required(ErrorMessage = "Vui lòng xác định duyệt hay từ chối.")]
        public bool? IsApproved { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }
}

