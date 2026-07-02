using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Service
{
    public class CreateServiceRequest
    {
        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(30)]
        public string Unit { get; set; } = "cái";

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
