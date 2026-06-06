using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Represents a sports complex containing multiple courts.</summary>
public class CourtComplex
{
  /// <summary>Primary key.</summary>
  public int ComplexId { get; set; }

  /// <summary>Display name for the complex.</summary>
  [Required, MaxLength(150)]
  public string ComplexName { get; set; } = string.Empty;

  /// <summary>Physical address of the complex.</summary>
  [Required, MaxLength(300)]
  public string Address { get; set; } = string.Empty;

  /// <summary>Contact phone number for the complex.</summary>
  [MaxLength(20)]
  public string? Phone { get; set; }

  /// <summary>Contact name of the manager.</summary>
  [MaxLength(100)]
  public string? ManagerName { get; set; }

  /// <summary>FK to the user managing this complex.</summary>
  public int? ManagerId { get; set; }

  /// <summary>Soft delete flag.</summary>
  public bool IsDeleted { get; set; } = false;

  /// <summary>Detailed description of the complex.</summary>
  [MaxLength(1000)]
  public string? Description { get; set; }

  /// <summary>Cover image URL for the complex.</summary>
  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>Last update timestamp.</summary>
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
  public User? Manager { get; set; }
  public ICollection<Court> Courts { get; set; } = new List<Court>();
}
