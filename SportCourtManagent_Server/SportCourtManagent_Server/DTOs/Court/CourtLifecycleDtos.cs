using System;
using System.Collections.Generic;

namespace SportCourtManagent_Server.DTOs.Court
{
    public class ScheduleCourtMaintenanceRequest
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Reason { get; set; }
        public bool ConfirmRefund { get; set; }
    }

    public class MaintenanceConflictPreviewDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int ConflictCount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public List<MaintenanceConflictBookingDto> Conflicts { get; set; } = [];
    }

    public class MaintenanceConflictBookingDto
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public decimal RefundAmount { get; set; }
        public string Status { get; set; } = "";
    }

    public class CourtLifecycleResultDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public int CancelledBookings { get; set; }
        public decimal TotalRefunded { get; set; }
    }
}
