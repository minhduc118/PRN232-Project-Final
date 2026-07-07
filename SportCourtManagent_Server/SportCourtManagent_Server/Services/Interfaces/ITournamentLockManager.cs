using System;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Services.Interfaces
{
  /// <summary>Interface for managing in-memory concurrency locks per court and date.</summary>
  public interface ITournamentLockManager
  {
    /// <summary>Acquires a lock for a specific court on a specific date asynchronous.</summary>
    /// <param name="courtId">The court ID.</param>
    /// <param name="bookingDate">The booking date.</param>
    /// <param name="timeout">Optional timeout to wait for the lock.</param>
    /// <returns>True if lock acquired, false if timed out.</returns>
    Task<bool> AcquireLockAsync(int courtId, DateTime bookingDate, TimeSpan? timeout = null);

    /// <summary>Releases the lock for a specific court on a specific date.</summary>
    /// <param name="courtId">The court ID.</param>
    /// <param name="bookingDate">The booking date.</param>
    void ReleaseLock(int courtId, DateTime bookingDate);
  }
}
