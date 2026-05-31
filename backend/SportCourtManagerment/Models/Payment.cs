using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Payment transaction associated with a court booking.</summary>
public class Payment
{
  /// <summary>Primary key.</summary>
  public int PaymentId { get; set; }

  /// <summary>FK to the booking being paid for.</summary>
  public int BookingId { get; set; }

  /// <summary>Total amount charged in VND.</summary>
  public decimal Amount { get; set; }

  /// <summary>Payment gateway or method used.</summary>
  public PaymentMethod PaymentMethod { get; set; }

  /// <summary>Unique transaction reference from payment gateway (nullable for cash).</summary>
  [MaxLength(200)]
  public string? TransactionId { get; set; }

  /// <summary>Raw JSON response from payment gateway for audit trail.</summary>
  public string? GatewayResponse { get; set; }

  /// <summary>Current transaction status.</summary>
  public PaymentStatus Status { get; set; }

  /// <summary>Amount refunded to customer in VND.</summary>
  public decimal RefundAmount { get; set; }

  /// <summary>Timestamp when refund was processed.</summary>
  public DateTime? RefundedAt { get; set; }

  /// <summary>Internal note about the refund reason.</summary>
  [MaxLength(300)]
  public string? RefundNote { get; set; }

  /// <summary>Timestamp when payment was successfully collected.</summary>
  public DateTime? PaidAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Booking Booking { get; set; } = null!;
  public Invoice? Invoice { get; set; }
}
