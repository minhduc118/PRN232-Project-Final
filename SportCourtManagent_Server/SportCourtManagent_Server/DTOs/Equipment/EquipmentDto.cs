using System;

namespace SportCourtManagent_Server.DTOs.Equipment
{
    public class EquipmentDto
    {
        public int InventoryId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}
