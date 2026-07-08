using ImageMapper.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Collections.Frozen;

namespace ImageMapper.Api.Services
{
    /// <summary>
    /// Fetches image information from configured folders and extracts their metadata, including geolocation if available
    /// </summary>
    public class ImageInfoFetcher(IConfiguration _config, IMemoryCache _cache, CacheSignal<ImageInfo> _cacheSignal)
    {
        /// <summary>
        /// Fetches a list of all image files from the configured folders
        /// </summary>
        /// <returns>An enumerable of image file paths</returns>
        public IEnumerable<BasicFileInfo> GetImageFiles()
        {
            if (_cache.TryGetValue("ImageFiles", out List<BasicFileInfo>? cachedImageFiles) && cachedImageFiles != null)
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
            await _cacheSignal.WaitAsync(ct);

            try
            {
                ResolveFoldersAndFiles();

                if (!_cache.TryGetValue("ImageFiles", out List<BasicFileInfo>? imageFiles) || imageFiles == null)
                {
                    return 0;
                }

                int imageCount = 0;

                foreach (BasicFileInfo imageFile in imageFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var imageInfo = ImageInfoHelpers.GetImageInfo(imageFile);

                    // Only cache images that have valid geolocation data (non-zero latitude and longitude)
                    if (imageInfo != null && imageInfo.Longitude != 0 && imageInfo.Latitude != 0)
                    {
                        _cache.Set(imageFile.Id, imageInfo);
                        imageCount++;
                    }
                }
                Log.Information("Cache updated with {Count} images", imageCount);

                _cache.Set("ImageCount", imageCount);

                return imageCount;
            }
            finally
            {
                _cacheSignal.Release();
            }
        }

        /// <summary>
        /// Fetches the image information for a specific image file from the cache, if available
        /// </summary>
        /// <param name="file">The image file for which to fetch information</param>
        /// <returns>The image information if available; otherwise, null</returns>
        public ImageInfo? GetImageInfo(BasicFileInfo file)
        {
            if (_cache.TryGetValue(file.Id, out ImageInfo? cachedImage) && cachedImage != null)
                return cachedImage;

            return null;
        }

        /// <summary>
        /// Returns the total count of processed images
        /// </summary>
        /// <returns>The total count of processed images</returns>
        public int GetImageCount()
        {
            if (_cache.TryGetValue("ImageCount", out int imageCount))
                return imageCount;

            return 0;
        }

        /// <summary>
        /// Initialises the fetcher by resolving folders and finding valid files in those folders
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void ResolveFoldersAndFiles()
        {
            var imageFolders = ImageFetcherHelpers.ResolveImageFolders(_config);
            if (imageFolders == null || imageFolders.Length == 0)
            {
                Log.Warning("No valid image folders found in configuration");
                return;
            }

            Log.Information("ImageInfoFetcher initialized with ImageFolders: {@ImageFolders}", imageFolders);

            List<BasicFileInfo> imageFiles = [.. ImageFetcherHelpers.GetImageList(imageFolders)
                .Select(f => new BasicFileInfo(ImageFetcherHelpers.GenerateIdForPath(f), Path.GetFileName(f), f))];

            _cache.Set("ImageFiles", imageFiles);

            Log.Information("ImageInfoFetcher found {ImageCount} image files in configured folders", imageFiles?.Count ?? 0);
        }
    }
}
