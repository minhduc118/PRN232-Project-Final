using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("Services")]
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQty { get; set; } = 0;

        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<EquipmentInventory> EquipmentInventories { get; set; } = new List<EquipmentInventory>();
    }
}

