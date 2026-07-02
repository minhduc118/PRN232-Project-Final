using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Review;


public class CreateReviewDto
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5.")]
    public byte Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment tối đa 1000 ký tự.")]
    public string? Comment { get; set; }
}
