using ImageMapper.Services.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ImageMapper.Services.Caching;

/// <summary>
/// Broadcasts newly cached images to subscribers using a bounded channel per subscriber
/// </summary>
internal sealed class ImageCacheNotifier : IImageCacheNotifier
{
    // Large enough for a typical full scan; a stalled subscriber drops items rather than growing memory unbounded
    private const int SubscriberCapacity = 100_000;

    private readonly ConcurrentDictionary<Guid, Channel<ImageInfo>> _subscribers = [];

    public void Publish(ImageInfo image)
    {
        foreach (var subscriber in _subscribers.Values)
        {
            subscriber.Writer.TryWrite(image);
        }
    }

    public IAsyncEnumerable<ImageInfo> Subscribe(CancellationToken ct = default)
    {
        var channel = Channel.CreateBounded<ImageInfo>(new BoundedChannelOptions(SubscriberCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var id = Guid.NewGuid();
        _subscribers[id] = channel;

        return ReadAsync(id, channel, ct);
    }

    private async IAsyncEnumerable<ImageInfo> ReadAsync(
        Guid id,
        Channel<ImageInfo> channel,
        [EnumeratorCancellation] CancellationToken ct)
    {
        try
        {
            await foreach (var image in channel.Reader.ReadAllAsync(ct))
            {
                yield return image;
            }
        }
        finally
        {
            _subscribers.TryRemove(id, out _);
        }
    }
}
