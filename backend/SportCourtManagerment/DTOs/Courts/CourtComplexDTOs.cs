using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Courts;

public class CourtComplexDto
{
  public int ComplexId { get; set; }
  public string ComplexName { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public string? Phone { get; set; }
  public string? ManagerName { get; set; }
  public int? ManagerId { get; set; }
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public int TotalCourts { get; set; }
  public int ActiveCourts { get; set; }
  public int MaintenanceCourts { get; set; }
  public int InactiveCourts { get; set; }
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

  [MaxLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự.")]
  public string? Phone { get; set; }

  [MaxLength(100)]
  public string? ManagerName { get; set; }

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

  [MaxLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự.")]
  public string? Phone { get; set; }

  [MaxLength(100)]
  public string? ManagerName { get; set; }

  public int? ManagerId { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }
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
