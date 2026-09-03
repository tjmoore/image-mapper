using ImageMapper.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Frozen;

namespace ImageMapper.Services
{
    /// <summary>
    /// Fetches image information from configured folders and extracts their metadata, including geolocation if available
    /// </summary>
    internal class ImageInfoFetcher(
        IConfiguration config,
        IMemoryCache cache,
        CacheSignal<ImageInfo> cacheSignal,
        ICacheActivityStatus cacheActivityStatus) : IImageInfoFetcher
    {
        /// <summary>
        /// Fetches a list of all image files from the configured folders
        /// </summary>
        /// <returns>An enumerable of image file paths</returns>
        public IEnumerable<BasicFileInfo> GetImageFiles()
        {
            if (cache.TryGetValue("ImageFiles", out List<BasicFileInfo>? cachedImageFiles) && cachedImageFiles != null)
            {
                return cachedImageFiles.ToFrozenSet();
            }

            return [];
        }

        /// <summary>
        /// Caches the image information for all image files found in the configured folders
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>The number of images processed</returns>
        public async Task<int> ProcessImagesAsync(CancellationToken ct)
        {
            await cacheSignal.WaitAsync(ct);

            try
            {
                ResolveFoldersAndFiles();

                if (!cache.TryGetValue("ImageFiles", out List<BasicFileInfo>? imageFiles) || imageFiles == null)
                {
                    cacheActivityStatus.MarkCachingStarted(0);
                    cacheActivityStatus.UpdateProgress(0, 0);
                    return 0;
                }

                int imageCount = 0;
                int processedFileCount = 0;
                int totalFileCount = imageFiles.Count;

                cacheActivityStatus.MarkCachingStarted(totalFileCount);
                cacheActivityStatus.UpdateProgress(processedFileCount, totalFileCount);

                foreach (BasicFileInfo imageFile in imageFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var imageInfo = ImageInfoHelpers.GetImageInfo(imageFile);

                    // All images cached even if they don't have geolocation data
                    if (imageInfo != null)
                    {
                        cache.Set(imageFile.Id, imageInfo);
                        imageCount++;
                    }

                    processedFileCount++;
                    cacheActivityStatus.UpdateProgress(processedFileCount, totalFileCount);
                }
                Log.Information("Cache updated with {Count} images", imageCount);

                cache.Set("ImageCount", imageCount);

                return imageCount;
            }
            finally
            {
                cacheActivityStatus.MarkCachingStopped();
                cacheSignal.Release();
            }
        }

        /// <summary>
        /// Fetches the image information for a specific image file from the cache, if available
        /// </summary>
        /// <param name="id">The ID of the image for which to fetch information</param>
        /// <returns>The image information if available; otherwise, null</returns>
        public ImageInfo? GetImageInfo(string id)
        {
            if (cache.TryGetValue(id, out ImageInfo? cachedImage) && cachedImage != null)
                return cachedImage;

            return null;
        }

        /// <summary>
        /// Returns the total count of processed images
        /// </summary>
        /// <returns>The total count of processed images</returns>
        public int GetImageCount()
        {
            if (cache.TryGetValue("ImageCount", out int imageCount))
                return imageCount;

            return 0;
        }

        /// <summary>
        /// Initialises the fetcher by resolving folders and finding valid files in those folders
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void ResolveFoldersAndFiles()
        {
            var imageFolders = ImageFetcherHelpers.ResolveImageFolders(config);
            if (imageFolders == null || imageFolders.Length == 0)
            {
                Log.Warning("No valid image folders found in configuration");
                return;
            }

            Log.Information("ImageInfoFetcher initialized with ImageFolders: {@ImageFolders}", imageFolders);

            List<BasicFileInfo> imageFiles = [.. ImageFetcherHelpers.GetImageList(imageFolders)
                .Select(f => new BasicFileInfo(ImageFetcherHelpers.GenerateIdForPath(f), Path.GetFileName(f), f))];

            cache.Set("ImageFiles", imageFiles);

            Log.Information("ImageInfoFetcher found {ImageCount} image files in configured folders", imageFiles?.Count ?? 0);
        }
    }
}
