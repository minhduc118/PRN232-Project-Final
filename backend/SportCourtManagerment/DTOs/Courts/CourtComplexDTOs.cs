using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Courts;

public class CourtComplexDto
{
  public int ComplexId { get; set; }
  public string ComplexName { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  /// <summary>Derived from Manager.Phone — not stored on complex itself.</summary>
  public string? Phone { get; set; }
  /// <summary>Derived from Manager.FullName — not stored on complex itself.</summary>
  public string? ManagerName { get; set; }
  public int? ManagerId { get; set; }
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public int TotalCourts { get; set; }
  public int ActiveCourts { get; set; }
  public int MaintenanceCourts { get; set; }
  public int InactiveCourts { get; set; }
  public List<int> CourtTypeIds { get; set; } = new();
  public DateTime CreatedAt { get; set; }
}

/// <summary>Booking summary for complex booking history tab.</summary>
public class BookingSummaryDto
{
  public int BookingId { get; set; }
  public string BookingCode { get; set; } = string.Empty;
  public int UserId { get; set; }
  public string? CustomerName { get; set; }
  public string? CustomerPhone { get; set; }
  public int CourtId { get; set; }
  public string? CourtName { get; set; }
  public string BookingDate { get; set; } = string.Empty;
  public string StartTime { get; set; } = string.Empty;
  public string EndTime { get; set; } = string.Empty;
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string? PaymentMethod { get; set; }
  public string? PaymentStatus { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class CreateComplexDto
{
  [Required(ErrorMessage = "Tên tổ hợp không được để trống.")]
  [MaxLength(150, ErrorMessage = "Tên tổ hợp không vượt quá 150 ký tự.")]
  public string ComplexName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Địa chỉ không được để trống.")]
  [MaxLength(300, ErrorMessage = "Địa chỉ không vượt quá 300 ký tự.")]
  public string Address { get; set; } = string.Empty;

  public int? ManagerId { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }
}

public class UpdateComplexDto
{
  [Required(ErrorMessage = "Tên tổ hợp không được để trống.")]
  [MaxLength(150, ErrorMessage = "Tên tổ hợp không vượt quá 150 ký tự.")]
  public string ComplexName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Địa chỉ không được để trống.")]
  [MaxLength(300, ErrorMessage = "Địa chỉ không vượt quá 300 ký tự.")]
  public string Address { get; set; } = string.Empty;

  public int? ManagerId { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }
}

/// <summary>Response DTO for image upload endpoints.</summary>
public class ImageUploadResultDto
{
  public string Url { get; set; } = string.Empty;
}

/// <summary>Kết quả phân trang cho danh sách tổ hợp sân.</summary>
public class PagedComplexResult
{
  public List<CourtComplexDto> Items      { get; set; } = new();
  public int                   TotalCount { get; set; }
  public int                   Page       { get; set; }
  public int                   PageSize   { get; set; }
  public int                   TotalPages { get; set; }
  public ComplexStatsDto       Stats      { get; set; } = new();
}

/// <summary>Thống kê tổng quan hệ thống sân.</summary>
public class ComplexStatsDto
{
  public int TotalComplexes    { get; set; }
  public int TotalCourts       { get; set; }
  public int ActiveCourts      { get; set; }
  public int MaintenanceCourts { get; set; }
  public int InactiveCourts    { get; set; }
}
