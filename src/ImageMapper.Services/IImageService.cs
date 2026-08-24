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
    /// Asynchronously retrieves the image data as a byte array from the specified image ID.
    /// </summary>
    /// <remarks>The image ID is a unique identifier generated from the image's full path.
    /// This prevents the frontend from accessing file system paths.</remarks>
    /// <param name="id">The unique image ID</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a byte array of the image data, or
    /// null if the image could not be found.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    Task<byte[]?> GetImageBytesAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the count of processed image files
    /// </summary>
    /// <returns>The count of processed image files</returns>
    int GetImageCount();
}
