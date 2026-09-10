using ImageMapper.Models;
using System.Runtime.CompilerServices;

namespace ImageMapper.Services;

public sealed class ImageService(IImageInfoFetcher imageInfoFetcher) : IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images</returns>
    public async IAsyncEnumerable<ImageInfo> GetImagesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        IEnumerable<BasicFileInfo> imageFiles = imageInfoFetcher.GetImageFiles();

        if (!imageFiles.Any())
            yield break;

        foreach (BasicFileInfo file in imageFiles)
        {
            ct.ThrowIfCancellationRequested();

            var image = imageInfoFetcher.GetImageInfo(file.Id);
            if (image != null)
                yield return image;
        }
    }

    /// <summary>
    /// Gets a read-only stream for the specified image file path. The caller is responsible for disposing the stream.
    /// </summary>
    /// <param name="id">The unique image ID</param>
    /// <returns>A read-only stream of the image file, or null if the file does not exist</returns>
    public Stream? GetImageStream(string id)
    {
        var image = imageInfoFetcher.GetImageInfo(id);

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
    public ImageInfo? GetImageInfo(string id) => imageInfoFetcher.GetImageInfo(id);

    /// <summary>
    /// Retrieves the count of processed image files.
    /// </summary>
    /// <returns>The count of processed image files.</returns>
    public int GetImageCount() => imageInfoFetcher.GetImageCount();
}
