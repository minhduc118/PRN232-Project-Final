using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class PaymentDto
  {
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string TransactionId { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime? PaidAt { get; set; }
  }

  public class BookingServiceItemDto
  {
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
  }

  public class BookingDto
  {
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public int UserId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int SlotId { get; set; }
    public string SlotName { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public int? PromotionId { get; set; }
    public string? PromotionCode { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentDto? Payment { get; set; }
    public List<BookingServiceItemDto> Services { get; set; } = new List<BookingServiceItemDto>();
  }
}
