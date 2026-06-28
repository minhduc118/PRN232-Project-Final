using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("MembershipTiers")]
    public class MembershipTier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TierId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TierName { get; set; } = null!;

        [Required]
        public int MinPoints { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercent { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

