using ImageMapper.Api.Exceptions;
using ImageMapper.Models;

namespace ImageMapper.Api.Services;

public interface IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information.
    /// </summary>
    /// <remarks>This method allows for cancellation of the operation through the provided cancellation token.
    /// If the operation is canceled, an <see cref="OperationCanceledException"/> will be thrown.</remarks>
    /// <param name="ct">The cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images.</returns>
    IAsyncEnumerable<ImageInfo> GetImagesAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves the image data as a byte array from the specified relative path.
    /// </summary>
    /// <remarks>This method is intended for use in scenarios where image data needs to be loaded
    /// asynchronously, such as in UI applications. Ensure that the relative path is correctly specified to avoid
    /// errors.</remarks>
    /// <param name="relativePath">The relative path to the image file. This path must be valid and accessible; otherwise, an exception may be
    /// thrown.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a byte array of the image data, or
    /// null if the image could not be found.</returns>
    /// <exception cref="PathTraversalException">Thrown when the provided relative path is invalid or attempts to traverse outside the allowed directory.</exception>"
    Task<byte[]?> GetImageBytesAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves the total count of image files.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The total count of image files.</returns>
    Task<int> GetImageCountAsync(CancellationToken ct = default);
}
