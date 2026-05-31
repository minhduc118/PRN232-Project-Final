namespace SportCourtManagerment.Models;

/// <summary>Line item linking a service to a specific booking (e.g. racket rental).</summary>
public class BookingService
{
  /// <summary>Primary key.</summary>
  public int BookingServiceId { get; set; }

  /// <summary>FK to parent booking.</summary>
  public int BookingId { get; set; }

  /// <summary>FK to selected service.</summary>
  public int ServiceId { get; set; }

  /// <summary>Quantity of service units ordered.</summary>
  public int Quantity { get; set; }

  /// <summary>Unit price at time of booking (snapshot to prevent price drift).</summary>
  public decimal UnitPrice { get; set; }

  /// <summary>Quantity × UnitPrice total for this line item.</summary>
  public decimal TotalPrice { get; set; }

  // Navigation properties
  public Booking Booking { get; set; } = null!;
  public Service Service { get; set; } = null!;
}
