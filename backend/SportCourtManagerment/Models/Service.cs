using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Add-on service available for booking (equipment rental, drinks, coaching).</summary>
public class Service
{
  /// <summary>Primary key.</summary>
  public int ServiceId { get; set; }

  /// <summary>Service display name.</summary>
  [Required, MaxLength(100)]
  public string ServiceName { get; set; } = string.Empty;

  /// <summary>Category group (Equipment / Drink / Coach / Event).</summary>
  [Required, MaxLength(50)]
  public string Category { get; set; } = string.Empty;

  /// <summary>Price per unit.</summary>
  public decimal Price { get; set; }

  /// <summary>Unit label displayed to customers (e.g. cây/giờ, chai).</summary>
  [Required, MaxLength(30)]
  public string Unit { get; set; } = "cái";

  /// <summary>Service details shown in catalog.</summary>
  [MaxLength(300)]
  public string? Description { get; set; }

  /// <summary>URL to service image.</summary>
  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  /// <summary>Minimum stock threshold for low-stock alerts.</summary>
  public int MinStock { get; set; }

  /// <summary>Whether the service is currently offered.</summary>
  public bool IsActive { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
  public ICollection<EquipmentInventory> EquipmentInventories { get; set; } = new List<EquipmentInventory>();
}
