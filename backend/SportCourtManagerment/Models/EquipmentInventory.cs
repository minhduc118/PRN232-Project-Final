using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Individual physical equipment item tracked in inventory.</summary>
public class EquipmentInventory
{
  /// <summary>Primary key.</summary>
  public int InventoryId { get; set; }

  /// <summary>FK to service this item belongs to.</summary>
  public int ServiceId { get; set; }

  /// <summary>Unique barcode / identifier for the physical item.</summary>
  [Required, MaxLength(50)]
  public string ItemCode { get; set; } = string.Empty;

  /// <summary>Current physical condition of the item.</summary>
  public EquipmentCondition Condition { get; set; }

  /// <summary>Date the item was purchased.</summary>
  public DateOnly? PurchaseDate { get; set; }

  /// <summary>Original purchase price in VND.</summary>
  public decimal? PurchasePrice { get; set; }

  /// <summary>Internal notes (e.g. damage description).</summary>
  [MaxLength(300)]
  public string? Note { get; set; }

  /// <summary>Whether the item is available for rental.</summary>
  public bool IsAvailable { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Service Service { get; set; } = null!;
}
