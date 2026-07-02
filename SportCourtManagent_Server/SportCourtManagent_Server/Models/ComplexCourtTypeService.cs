using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("ComplexCourtTypeServices")]
    public class ComplexCourtTypeService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OfferingId { get; set; }

        [Required]
        public int ComplexId { get; set; }

        [ForeignKey("ComplexId")]
        public CourtComplex Complex { get; set; } = null!;

        [Required]
        public int CourtTypeId { get; set; }

        [ForeignKey("CourtTypeId")]
        public CourtType CourtType { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQty { get; set; }

        [Required]
        public ServiceMode ServiceMode { get; set; } = ServiceMode.Optional;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
