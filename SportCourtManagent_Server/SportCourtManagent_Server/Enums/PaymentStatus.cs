namespace SportCourtManagent_Server.Enums;

/// <summary>Payment transaction lifecycle status.</summary>
public enum PaymentStatus
{
  Pending,
  Success,
  Failed,
  Refunded,
  PartialRefund
}

