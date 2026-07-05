namespace SportCourtManagent_Server.DTOs.Maintenance
{
    public class MaintenanceCourtResponse
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public string CourtCode { get; set; } = string.Empty;
        public string CourtTypeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
