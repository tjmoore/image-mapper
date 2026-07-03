using ImageMapper.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Runtime.CompilerServices;

namespace ImageMapper.Api.Services;

public sealed class ImageService(
    IMemoryCache _cache,
    CacheSignal<ImageInfo> _imageCacheSignal) : IImageService
{
    /// <summary>
    /// Asynchronously retrieves a sequence of image information.
    /// </summary>
    /// <remarks>This method allows for cancellation of the operation through the provided cancellation token.
    /// If the operation is canceled, an <see cref="OperationCanceledException"/> will be thrown.</remarks>
    /// <param name="ct">The cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>An asynchronous sequence of <see cref="ImageInfo"/> objects representing the retrieved images.</returns>
    public async IAsyncEnumerable<ImageInfo> GetImagesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        try
        {            
            await _imageCacheSignal.WaitAsync(ct);

            if (!_cache.TryGetValue("ImageCacheInfo", out ImageCacheInfo? cacheInfo) || cacheInfo == null || cacheInfo.Keys.Count == 0)
                yield break;

            foreach (var key in cacheInfo.Keys)
            {
                ct.ThrowIfCancellationRequested();

                if (await _cache.GetOrCreateAsync(
                    key, _ =>
                    {
                        Log.Warning("GetImagesAsync - This should never happen!");
                        return Task.FromResult(default(ImageInfo));
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

        return await ImageFetcherHelpers.GetImageBytesAsync(image.FileName, ct);
    }

    /// <summary>
    /// Asynchronously retrieves the total count of image files.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The total count of image files.</returns>
    public async Task<int> GetImageCountAsync(CancellationToken ct = default)
    {
        ImageCacheInfo? cacheInfo = await _cache.GetOrCreateAsync(
        "ImageCacheInfo", _ =>
        {
            Log.Warning("GetImageCountAsync - This should never happen!");
            return Task.FromResult(default(ImageCacheInfo));
        });

        return cacheInfo?.TotalImageFiles ?? 0;
    }
}
