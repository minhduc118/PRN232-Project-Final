namespace SportCourtManagent_Server.DTOs.Court
{
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
        public List<int> CourtTypeIds { get; set; } = [];
        public DateTime CreatedAt { get; set; }
    }

    public class ComplexStatsDto
    {
        public int TotalComplexes { get; set; }
        public int TotalCourts { get; set; }
        public int ActiveCourts { get; set; }
        public int MaintenanceCourts { get; set; }
        public int InactiveCourts { get; set; }
    }

    public class PagedComplexResult
    {
        public List<CourtComplexDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public ComplexStatsDto Stats { get; set; } = new();
    }

    public class UpsertCourtComplexRequest
    {
        public string ComplexName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ImageUploadResultDto
    {
        public string Url { get; set; } = string.Empty;
    }

    public class CourtTypeDto
    {
        public int CourtTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CourtDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public string CourtCode { get; set; } = string.Empty;
        public int CourtTypeId { get; set; }
        public string CourtTypeName { get; set; } = string.Empty;
        public int ComplexId { get; set; }
        public string? ComplexName { get; set; }
        public string Status { get; set; } = "Available";
        public string OpenTime { get; set; } = "06:00";
        public string CloseTime { get; set; } = "22:00";
        public decimal PricePerHour { get; set; }
        public string? CourtSize { get; set; }
        public string? ImageUrl { get; set; }
    }
}
