using System.ComponentModel.DataAnnotations;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.ComplexService
{
    public class ComplexCourtTypeServiceDto
    {
        public int OfferingId { get; set; }
        public int ComplexId { get; set; }
        public int CourtTypeId { get; set; }
        public string CourtTypeName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public string ServiceMode { get; set; } = "Optional";
        public bool IsActive { get; set; }
    }

    public class CreateComplexCourtTypeServiceRequest
    {
        [Required]
        public int ServiceId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQty { get; set; }

        [Required]
        public ServiceMode ServiceMode { get; set; } = ServiceMode.Optional;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateComplexCourtTypeServiceRequest
    {
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQty { get; set; }

        [Required]
        public ServiceMode ServiceMode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
