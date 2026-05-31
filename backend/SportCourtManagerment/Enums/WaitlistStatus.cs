namespace SportCourtManagerment.Enums;

/// <summary>Waitlist entry status with FIFO notification flow.</summary>
public enum WaitlistStatus
{
  Waiting,
  Notified,
  Confirmed,
  Expired,
  Cancelled
}
