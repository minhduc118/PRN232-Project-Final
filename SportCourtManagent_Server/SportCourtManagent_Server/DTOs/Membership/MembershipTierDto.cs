namespace SportCourtManagent_Server.DTOs.Membership
{
    public class MembershipTierDto
    {
        public int TierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
