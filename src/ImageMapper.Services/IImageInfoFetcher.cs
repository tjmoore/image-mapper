using ImageMapper.Models;

namespace ImageMapper.Services
{
    public interface IImageInfoFetcher
    {
        /// <summary>
        /// Fetches a list of all image files from the configured folders
        /// </summary>
        /// <returns>An enumerable of image file paths</returns>
        public IEnumerable<BasicFileInfo> GetImageFiles();

        /// <summary>
        /// Caches the image information for all image files found in the configured folders
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The number of images processed</returns>
        public Task<int> ProcessImagesAsync(CancellationToken ct);

        /// <summary>
        /// Fetches the image information for a specific image file from the cache, if available
        /// </summary>
        /// <param name="id">The ID of the image for which to fetch information</param>
        /// <returns>The image information if available; otherwise, null</returns>
        public ImageInfo? GetImageInfo(string id);

        /// <summary>
        /// Returns the total count of processed images
        /// </summary>
        /// <returns>The total count of processed images</returns>
        public int GetImageCount();
    }
}
