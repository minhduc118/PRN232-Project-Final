using SportCourtManagerment.Enums;

namespace SportCourtManagerment.DTOs.Bookings;

public class BookingAdminDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public int? CourtTypeId { get; set; }
    
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    
    public DateOnly BookingDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public BookingStatus Status { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
