using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
  /// <summary>Manages in-memory semaphore locks per court and booking date to prevent race conditions.</summary>
  public class TournamentLockManager : ITournamentLockManager
  {
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ILogger<TournamentLockManager> _logger;

    public TournamentLockManager(ILogger<TournamentLockManager> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Generates a unique lock key for a court and date.</summary>
    private static string GetLockKey(int courtId, DateTime date) => $"court_{courtId}_{date:yyyyMMdd}";

    /// <summary>Acquires a lock for a specific court on a specific date asynchronous.</summary>
    public async Task<bool> AcquireLockAsync(int courtId, DateTime bookingDate, TimeSpan? timeout = null)
    {
      var key = GetLockKey(courtId, bookingDate);
      var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
      var waitTime = timeout ?? TimeSpan.FromSeconds(15);

      _logger.LogInformation($"Attempting to acquire lock for {key}");
      var acquired = await semaphore.WaitAsync(waitTime);
      if (!acquired)
      {
        _logger.LogWarning($"Timeout acquiring lock for {key} after {waitTime.TotalSeconds}s");
      }
      return acquired;
    }

    /// <summary>Releases the lock for a specific court on a specific date.</summary>
    public void ReleaseLock(int courtId, DateTime bookingDate)
    {
      var key = GetLockKey(courtId, bookingDate);
      if (_locks.TryGetValue(key, out var semaphore))
      {
        try
        {
          semaphore.Release();
          _logger.LogInformation($"Released lock for {key}");
        }
        catch (SemaphoreFullException ex)
        {
          _logger.LogWarning(ex, $"Semaphore already released for {key}");
        }
      }
    }
  }
}
