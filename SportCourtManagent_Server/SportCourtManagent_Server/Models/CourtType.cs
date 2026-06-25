using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("CourtTypes")]
    public class CourtType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourtTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Court> Courts { get; set; } = new List<Court>();
    }
}

