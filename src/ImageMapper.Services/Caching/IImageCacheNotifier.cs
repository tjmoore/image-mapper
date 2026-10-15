using ImageMapper.Services.Models;

namespace ImageMapper.Services.Caching;

/// <summary>
/// Pushes newly cached images to interested subscribers
/// </summary>
public interface IImageCacheNotifier
{
    /// <summary>
    /// Publishes a newly cached image to all current subscribers
    /// </summary>
    /// <param name="image">The image that was added to the cache</param>
    void Publish(ImageInfo image);

    /// <summary>
    /// Subscribes to images cached after the subscription is made. The subscription ends when the token is cancelled.
    /// </summary>
    /// <param name="ct">A cancellation token that ends the subscription</param>
    /// <returns>An asynchronous sequence of newly cached images</returns>
    IAsyncEnumerable<ImageInfo> Subscribe(CancellationToken ct = default);
}
