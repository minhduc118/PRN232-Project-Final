using System;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Equipment
{
    public class CreateEquipmentRequest
    {
        [Required(ErrorMessage = "Dịch vụ liên kết là bắt buộc.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Mã thiết bị/dụng cụ là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Mã không được vượt quá 50 ký tự.")]
        public string ItemCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tình trạng là bắt buộc.")]
        [RegularExpression("^(Good|Damaged|Retired)$", ErrorMessage = "Tình trạng không hợp lệ.")]
        public string Condition { get; set; } = "Good";

        [Required(ErrorMessage = "Ngày mua là bắt buộc.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Giá mua là bắt buộc.")]
        [Range(0, 1000000000, ErrorMessage = "Giá mua không hợp lệ.")]
        public decimal PurchasePrice { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
