using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromotionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PromoCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PromoName { get; set; } = null!;

        [Required]
        public DiscountType DiscountType { get; set; } = DiscountType.Percent;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

