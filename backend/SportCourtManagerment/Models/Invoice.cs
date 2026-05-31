using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Official invoice document generated after successful payment.</summary>
public class Invoice
{
  /// <summary>Primary key.</summary>
  public int InvoiceId { get; set; }

  /// <summary>Human-readable invoice number (e.g. INV-20260514-0001).</summary>
  [Required, MaxLength(30)]
  public string InvoiceNumber { get; set; } = string.Empty;

  /// <summary>FK to the associated booking.</summary>
  public int BookingId { get; set; }

  /// <summary>FK to the associated payment.</summary>
  public int PaymentId { get; set; }

  /// <summary>Subtotal before discounts in VND.</summary>
  public decimal SubTotal { get; set; }

  /// <summary>Total discount amount in VND.</summary>
  public decimal DiscountAmount { get; set; }

  /// <summary>VAT percentage applied (0 = no VAT).</summary>
  public decimal VatPercent { get; set; }

  /// <summary>Calculated VAT amount in VND.</summary>
  public decimal VatAmount { get; set; }

  /// <summary>Final amount including VAT minus discounts.</summary>
  public decimal TotalAmount { get; set; }

  /// <summary>URL to generated PDF invoice file.</summary>
  [MaxLength(500)]
  public string? PdfUrl { get; set; }

  /// <summary>Whether the invoice has been emailed to the customer.</summary>
  public bool IsEmailSent { get; set; }

  /// <summary>Timestamp when invoice email was sent.</summary>
  public DateTime? EmailSentAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Booking Booking { get; set; } = null!;
  public Payment Payment { get; set; } = null!;
}
