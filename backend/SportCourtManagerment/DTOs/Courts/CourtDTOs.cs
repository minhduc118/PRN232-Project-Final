using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Courts;

public class CourtDto
{
  public int CourtId { get; set; }
  public string CourtName { get; set; } = string.Empty;
  public string CourtCode { get; set; } = string.Empty;
  public int CourtTypeId { get; set; }
  public string CourtTypeName { get; set; } = string.Empty;
  public int? ComplexId { get; set; }
  public string? ComplexName { get; set; }
  public string? Description { get; set; }
  public string? Location { get; set; }
  public int? Capacity { get; set; }
  public string? Surface { get; set; }
  public string? ImageUrl { get; set; }
  public string Status { get; set; } = string.Empty;
  public string OpenTime { get; set; } = string.Empty; // "HH:mm"
  public string CloseTime { get; set; } = string.Empty; // "HH:mm"
  public decimal PricePerHour { get; set; }
  public string? CourtSize { get; set; }
  public List<string> ImageUrls { get; set; } = new List<string>();
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}

public class CreateCourtDto
{
  [Required(ErrorMessage = "Tên sân không được để trống.")]
  [MaxLength(100, ErrorMessage = "Tên sân không vượt quá 100 ký tự.")]
  public string CourtName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Mã sân không được để trống.")]
  [MaxLength(20, ErrorMessage = "Mã sân không vượt quá 20 ký tự.")]
  public string CourtCode { get; set; } = string.Empty;

  [Required(ErrorMessage = "Loại sân không được để trống.")]
  public int CourtTypeId { get; set; }

  public int? ComplexId { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  [MaxLength(300)]
  public string? Location { get; set; }

  public int? Capacity { get; set; }

  [MaxLength(100)]
  public string? Surface { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  [Required(ErrorMessage = "Trạng thái không được để trống.")]
  public string Status { get; set; } = "Available"; // "Available", "Booked", "InUse", "Maintenance", "Inactive"

  [Required(ErrorMessage = "Giờ mở cửa không được để trống.")]
  public string OpenTime { get; set; } = "06:00"; // "HH:mm"

  [Required(ErrorMessage = "Giờ đóng cửa không được để trống.")]
  public string CloseTime { get; set; } = "22:00"; // "HH:mm"

  [Required(ErrorMessage = "Giá thuê cơ bản không được để trống.")]
  [Range(0, double.MaxValue, ErrorMessage = "Giá thuê cơ bản phải lớn hơn hoặc bằng 0.")]
  public decimal PricePerHour { get; set; }

  [MaxLength(50)]
  public string? CourtSize { get; set; }

  public List<string> ImageUrls { get; set; } = new List<string>();
}

public class UpdateCourtDto
{
  [Required(ErrorMessage = "Tên sân không được để trống.")]
  [MaxLength(100, ErrorMessage = "Tên sân không vượt quá 100 ký tự.")]
  public string CourtName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Mã sân không được để trống.")]
  [MaxLength(20, ErrorMessage = "Mã sân không vượt quá 20 ký tự.")]
  public string CourtCode { get; set; } = string.Empty;

  [Required(ErrorMessage = "Loại sân không được để trống.")]
  public int CourtTypeId { get; set; }

  public int? ComplexId { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  [MaxLength(300)]
  public string? Location { get; set; }

  public int? Capacity { get; set; }

  [MaxLength(100)]
  public string? Surface { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  [Required(ErrorMessage = "Trạng thái không được để trống.")]
  public string Status { get; set; } = "Available";

  [Required(ErrorMessage = "Giờ mở cửa không được để trống.")]
  public string OpenTime { get; set; } = "06:00";

  [Required(ErrorMessage = "Giờ đóng cửa không được để trống.")]
  public string CloseTime { get; set; } = "22:00";

  [Required(ErrorMessage = "Giá thuê cơ bản không được để trống.")]
  [Range(0, double.MaxValue, ErrorMessage = "Giá thuê cơ bản phải lớn hơn hoặc bằng 0.")]
  public decimal PricePerHour { get; set; }

  [MaxLength(50)]
  public string? CourtSize { get; set; }

  public List<string> ImageUrls { get; set; } = new List<string>();
}
