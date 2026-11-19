using ImageMapper.Services.Caching;
using ImageMapper.Services.Models;
using ImageMapper.Services.Utils;
using System.Runtime.CompilerServices;

namespace ImageMapper.Services;

public sealed class ImageService(IImageInfoFetcher imageInfoFetcher, IImageCacheNotifier imageCacheNotifier) : IImageService
{
    /// <summary>
    /// Subscribes to images as they are added to the cache. Only images cached after subscribing are returned.
    /// </summary>
    /// <param name="ct">A cancellation token that ends the subscription</param>
    /// <returns>An asynchronous sequence of newly cached images</returns>
    public IAsyncEnumerable<ImageInfo> SubscribeToNewImagesAsync(CancellationToken ct = default) =>
        imageCacheNotifier.Subscribe(ct);

    /// <summary>
    /// Asynchronously retrieves a sequence of image information
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images</returns>
    public async IAsyncEnumerable<ImageInfo> GetImagesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        IReadOnlyList<BasicFileInfo> imageFiles = await imageInfoFetcher.GetImageFilesAsync(ct);

        if (imageFiles.Count == 0)
            yield break;

        foreach (BasicFileInfo file in imageFiles)
        {
            ct.ThrowIfCancellationRequested();

            var image = await imageInfoFetcher.GetImageInfoAsync(file.Id, ct);
            if (image != null)
                yield return image;
        }
    }

    /// <summary>
    /// Gets a read-only stream for the specified image file path. The caller is responsible for disposing the stream.
    /// </summary>
    /// <param name="id">The unique image ID</param>
    /// <returns>A read-only stream of the image file, or null if the file does not exist</returns>
    public async Task<Stream?> GetImageStreamAsync(string id, CancellationToken ct = default)
    {
        var image = await imageInfoFetcher.GetImageInfoAsync(id, ct);

        if (image != null)
        {
            return ImageFetcherHelpers.GetImageStream(image.FilePath);
        }

        return null;
    }

    /// <summary>
    /// Retrieves the image information for the specified image ID.
    /// </summary>
    /// <param name="id">The unique image ID</param>
    /// <returns>The image information if available; otherwise, null</returns>
    public Task<ImageInfo?> GetImageInfoAsync(string id, CancellationToken ct = default) => imageInfoFetcher.GetImageInfoAsync(id, ct);

    /// <summary>
    /// Retrieves the count of processed image files.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
    /// <returns>The count of processed image files.</returns>
    public Task<int> GetImageCountAsync(CancellationToken ct = default) => imageInfoFetcher.GetImageCountAsync(ct);
}
