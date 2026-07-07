using ImageMapper.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace ImageMapper.Api.Services;

public sealed class ImageService(
    IMemoryCache _cache,
    ImageInfoFetcher _imageInfoFetcher,
    CacheSignal<ImageInfo> _imageCacheSignal) : IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information.
    /// </summary>
    /// <remarks>This method allows for cancellation of the operation through the provided cancellation token.
    /// If the operation is canceled, an <see cref="OperationCanceledException"/> will be thrown.</remarks>
    /// <param name="reinitialise">Indicates whether to reinitialise the image fetcher before retrieving images.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images.</returns>
    public async IAsyncEnumerable<ImageInfo> GetImagesAsync(bool reinitialise = false, [EnumeratorCancellation] CancellationToken ct = default)
    {
        try
        {            
            await _imageCacheSignal.WaitAsync(ct);

            if (reinitialise)
            {
                _imageInfoFetcher.Reinitialise();

                // Clear cache - may have issues if IMemoryCache is not MemoryCache
                if (_cache is MemoryCache memoryCache)
                    memoryCache.Clear();
            }

            IEnumerable<BasicFileInfo> imageFiles = _imageInfoFetcher.GetImageFiles();

            if (!imageFiles.Any())
                yield break;

            foreach (BasicFileInfo file in imageFiles)
            {
                ct.ThrowIfCancellationRequested();

                // TODO: if GetImageInfo becomes async, consider GetOrCreateAsync
                if (_cache.GetOrCreate(file.Id, _ =>
                    {
                        return _imageInfoFetcher.GetImageInfo(file);
                    }) is ImageInfo image)
                {
                    yield return image;
                }
            }
        }
        finally
        {
            _imageCacheSignal.Release();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the image data as a byte array from the specified image ID.
    /// </summary>
    /// <remarks>The image ID is a unique identifier generated from the image's full path.
    /// This prevents the frontend from accessing file system paths.</remarks>
    /// <param name="id">The unique image ID</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a byte array of the image data, or
    /// null if the image could not be found.</returns>
    public async Task<byte[]?> GetImageBytesAsync(string id, CancellationToken ct = default)
    {
        if (!_cache.TryGetValue(id, out ImageInfo? image) || image == null)
            return null;

        return await ImageFetcherHelpers.GetImageBytesAsync(image.FilePath, ct);
    }

    /// <summary>
    /// Retrieves the total count of image files.
    /// </summary>
    /// <returns>The total count of image files.</returns>
    public int GetImageCount() => _imageInfoFetcher.GetImageCount();
}
