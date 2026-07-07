using ImageMapper.Models;

namespace ImageMapper.Api.Services;

public interface IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information.
    /// </summary>
    /// <remarks>This method allows for cancellation of the operation through the provided cancellation token.
    /// If the operation is canceled, an <see cref="OperationCanceledException"/> will be thrown.</remarks>
    /// <param name="reinitialise">Indicates whether to reinitialise the image fetcher before retrieving images.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images.</returns>
    IAsyncEnumerable<ImageInfo> GetImagesAsync(bool reinitialise = false, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves the image data as a byte array from the specified image ID.
    /// </summary>
    /// <remarks>The image ID is a unique identifier generated from the image's full path.
    /// This prevents the frontend from accessing file system paths.</remarks>
    /// <param name="id">The unique image ID</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a byte array of the image data, or
    /// null if the image could not be found.</returns>
    Task<byte[]?> GetImageBytesAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the total count of image files.
    /// </summary>
    /// <returns>The total count of image files.</returns>
    int GetImageCount();
}
