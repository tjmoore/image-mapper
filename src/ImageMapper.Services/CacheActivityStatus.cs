using ImageMapper.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ImageMapper.Services;

/// <summary>
/// Represents the status of caching activities, including whether caching is currently active, the number of processed files, and the total number of files to be processed.
/// Provides methods to mark the start and stop of caching, update progress, and stream status updates to subscribers.
/// </summary>
internal sealed class CacheActivityStatus : ICacheActivityStatus
{
    private int _activeCacheOperations;
    private int _processedFileCount;
    private int _totalFileCount;
    private readonly ConcurrentDictionary<Guid, Channel<CacheStatusInfo>> _subscribers = [];

    /// <summary>
    /// Gets a value indicating whether caching activities are currently active.
    /// </summary>
    public bool IsCaching => Volatile.Read(ref _activeCacheOperations) > 0;

    /// <summary>
    /// Gets the current status of caching activities.
    /// </summary>
    /// <returns>A <see cref="CacheStatusInfo"/> object representing the current status of caching activities.</returns>
    public CacheStatusInfo GetStatus()
    {
        return new CacheStatusInfo(
            IsCaching,
            Volatile.Read(ref _processedFileCount),
            Volatile.Read(ref _totalFileCount));
    }

    /// <summary>
    /// Marks the start of caching activities.
    /// </summary>
    /// <param name="totalFileCount">The total number of files to be processed.</param>
    public void MarkCachingStarted(int totalFileCount)
    {
        var previousCount = Interlocked.Increment(ref _activeCacheOperations) - 1;
        if (previousCount == 0)
        {
            Interlocked.Exchange(ref _processedFileCount, 0);
            Interlocked.Exchange(ref _totalFileCount, Math.Max(0, totalFileCount));
            PublishStatus(GetStatus());
        }
    }

    /// <summary>
    /// Updates the progress of caching activities by setting the number of processed files and the total number of files to be processed.
    /// </summary>
    /// <param name="processedFileCount">The number of files that have been processed.</param>
    /// <param name="totalFileCount">The total number of files to be processed.</param>
    public void UpdateProgress(int processedFileCount, int totalFileCount)
    {
        if (!IsCaching)
        {
            return;
        }

        Interlocked.Exchange(ref _processedFileCount, Math.Max(0, processedFileCount));
        Interlocked.Exchange(ref _totalFileCount, Math.Max(0, totalFileCount));
        PublishStatus(GetStatus());
    }

    /// <summary>
    /// Marks the stop of caching activities. If there are no remaining active cache operations, it publishes the updated status indicating that caching has stopped.
    /// </summary>
    public void MarkCachingStopped()
    {
        var remainingCount = Interlocked.Decrement(ref _activeCacheOperations);
        if (remainingCount < 0)
        {
            Interlocked.Exchange(ref _activeCacheOperations, 0);
            remainingCount = 0;
        }

        if (remainingCount == 0)
        {
            PublishStatus(GetStatus() with { IsCaching = false });
        }
    }

    /// <summary>
    /// Streams the current status of caching activities to subscribers. Each subscriber receives updates whenever the status changes.
    /// The method returns an asynchronous enumerable that can be consumed to receive status updates in real-time.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
    /// <returns>An asynchronous enumerable of <see cref="CacheStatusInfo"/> objects representing the status updates.</returns>
    public IAsyncEnumerable<CacheStatusInfo> StreamStatuses(CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<CacheStatusInfo>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var subscriberId = Guid.NewGuid();
        _subscribers[subscriberId] = channel;
        channel.Writer.TryWrite(GetStatus());

        return ReadStatusesAsync(subscriberId, channel, ct);
    }

    private async IAsyncEnumerable<CacheStatusInfo> ReadStatusesAsync(
        Guid subscriberId,
        Channel<CacheStatusInfo> channel,
        [EnumeratorCancellation] CancellationToken ct)
    {
        try
        {
            await foreach (var status in channel.Reader.ReadAllAsync(ct))
            {
                yield return status;
            }
        }
        finally
        {
            _subscribers.TryRemove(subscriberId, out _);
        }
    }

    private void PublishStatus(CacheStatusInfo status)
    {
        foreach (var subscriber in _subscribers.Values)
        {
            subscriber.Writer.TryWrite(status);
        }
    }
}
