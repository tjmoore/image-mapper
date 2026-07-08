using ImageMapper.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ImageMapper.Api.Services;

public sealed class CacheActivityStatus
{
    private int _activeCacheOperations;
    private readonly ConcurrentDictionary<Guid, Channel<CacheStatusInfo>> _subscribers = [];

    public bool IsCaching => Volatile.Read(ref _activeCacheOperations) > 0;

    public void MarkCachingStarted()
    {
        var previousCount = Interlocked.Increment(ref _activeCacheOperations) - 1;
        if (previousCount == 0)
        {
            PublishStatus(new CacheStatusInfo(IsCaching: true));
        }
    }

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
            PublishStatus(new CacheStatusInfo(IsCaching: false));
        }
    }

    public IAsyncEnumerable<CacheStatusInfo> StreamStatuses(CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<CacheStatusInfo>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var subscriberId = Guid.NewGuid();
        _subscribers[subscriberId] = channel;
        channel.Writer.TryWrite(new CacheStatusInfo(IsCaching));

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
