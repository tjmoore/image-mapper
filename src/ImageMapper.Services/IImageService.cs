using ImageMapper.Models;

namespace ImageMapper.Services;

public interface IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images</returns>
    IAsyncEnumerable<ImageInfo> GetImagesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a read-only stream for the specified image file ID. The caller is responsible for disposing the stream.
    /// </summary>
    /// <remarks>The image ID is a unique identifier generated from the image's full path.
    /// This prevents the frontend from accessing file system paths.</remarks>
    /// <param name="id">The unique image ID</param>
    /// <returns>A read-only stream of the image file, or null if the file does not exist</returns>
    Stream? GetImageStream(string id);

    /// <summary>
    /// Retrieves the image information for the specified image ID.
    /// </summary>
    /// <param name="id">The unique image ID</param>
    /// <returns>The image information if available; otherwise, null</returns>
    ImageInfo? GetImageInfo(string id);

    /// <summary>
    /// Retrieves the count of processed image files
    /// </summary>
    /// <returns>The count of processed image files</returns>
    int GetImageCount();
}
