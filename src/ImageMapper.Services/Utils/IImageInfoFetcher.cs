using ImageMapper.Services.Models;

namespace ImageMapper.Services.Utils
{
    public interface IImageInfoFetcher
    {
        /// <summary>
        /// Fetches a list of all known image files from the cache
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>A read-only list of image file information</returns>
        public Task<IReadOnlyList<BasicFileInfo>> GetImageFilesAsync(CancellationToken ct = default);

        /// <summary>
        /// Loads previously cached image data (for example persisted in a distributed cache) into the in-memory cache
        /// so it is available before the first scan of the image folders completes
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The number of images loaded from the cache</returns>
        public Task<int> LoadFromCacheAsync(CancellationToken ct);

        /// <summary>
        /// Scans the configured folders and caches image information. Files unchanged since they were last cached are not reprocessed,
        /// and cached entries for files that no longer exist are removed.
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The number of images in the cache after processing</returns>
        public Task<int> ProcessImagesAsync(CancellationToken ct);

        /// <summary>
        /// Fetches the image information for a specific image file from the cache, if available
        /// </summary>
        /// <param name="id">The ID of the image for which to fetch information</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The image information if available; otherwise, null</returns>
        public Task<ImageInfo?> GetImageInfoAsync(string id, CancellationToken ct = default);

        /// <summary>
        /// Returns the total count of processed images
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The total count of processed images</returns>
        public Task<int> GetImageCountAsync(CancellationToken ct = default);
    }
}
