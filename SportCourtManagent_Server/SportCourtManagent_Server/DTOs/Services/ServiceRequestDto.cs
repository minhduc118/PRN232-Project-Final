using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Services
{
    public class ServiceRequestDto
    {
        [Required(ErrorMessage = "Service Name is required.")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters.")]
        public string ServiceName { get; set; } = null!;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative integer.")]
        public int StockQty { get; set; }
    }
}
