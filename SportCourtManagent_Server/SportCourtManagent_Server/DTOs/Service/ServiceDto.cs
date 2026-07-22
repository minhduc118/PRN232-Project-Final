namespace SportCourtManagent_Server.DTOs.Service
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = "cái";
        public string? Description { get; set; }
        public int StockQty { get; set; }
        public bool IsActive { get; set; }
    }
}
