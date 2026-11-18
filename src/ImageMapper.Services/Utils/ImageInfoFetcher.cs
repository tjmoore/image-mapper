using ImageMapper.Services.Caching;
using ImageMapper.Services.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ImageMapper.Services.Utils
{
    /// <summary>
    /// Fetches image information from configured folders and extracts their metadata, including geolocation if available.
    /// Results are held in a <see cref="HybridCache"/> (in-memory plus an optional distributed cache such as Redis for persistence).
    /// </summary>
    internal class ImageInfoFetcher(
        IConfiguration config,
        HybridCache cache,
        CacheSignal<ImageInfo> cacheSignal,
        ICacheActivityStatus cacheActivityStatus,
        IImageCacheNotifier imageCacheNotifier) : IImageInfoFetcher
    {
        internal const string FilesKey = "imagemapper:files";
        internal const string CountKey = "imagemapper:count";
        internal static string ImageKey(string id) => $"imagemapper:image:{id}";

        private static readonly TimeSpan CacheLifetime = TimeSpan.FromDays(30);

        private static readonly HybridCacheEntryOptions EntryOptions = new()
        {
            Expiration = CacheLifetime,
            LocalCacheExpiration = CacheLifetime
        };

        // Lookup only: the factory is never run and a miss returns the default value
        private static readonly HybridCacheEntryOptions LookupOptions = new()
        {
            Flags = HybridCacheEntryFlags.DisableUnderlyingData,
            Expiration = CacheLifetime,
            LocalCacheExpiration = CacheLifetime
        };

        /// <inheritdoc />
        public async Task<IReadOnlyList<BasicFileInfo>> GetImageFilesAsync(CancellationToken ct = default) =>
            await TryGetAsync<List<BasicFileInfo>>(FilesKey, ct) ?? [];

        /// <inheritdoc />
        public async Task<int> LoadFromCacheAsync(CancellationToken ct)
        {
            var files = await GetImageFilesAsync(ct);
            int loaded = 0;

            // Reading each entry pulls it from the distributed cache into the in-memory cache
            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();

                if (await TryGetAsync<ImageInfo>(ImageKey(file.Id), ct) != null)
                    loaded++;
            }

            Log.Information("Loaded {Count} of {Total} images from cache", loaded, files.Count);
            return loaded;
        }

        /// <inheritdoc />
        public async Task<int> ProcessImagesAsync(CancellationToken ct)
        {
            await cacheSignal.WaitAsync(ct);

            try
            {
                var previousFiles = await GetImageFilesAsync(ct);
                var imageFiles = ScanImageFiles();

                if (imageFiles.Count == 0 && previousFiles.Count == 0)
                {
                    cacheActivityStatus.MarkCachingStarted(0);
                    cacheActivityStatus.UpdateProgress(0, 0);
                    return 0;
                }

                // Publish the manifest first so consumers can read already cached images while the scan progresses
                await cache.SetAsync(FilesKey, imageFiles, EntryOptions, cancellationToken: ct);

                int imageCount = 0;
                int changedCount = 0;
                int processedFileCount = 0;
                int totalFileCount = imageFiles.Count;

                cacheActivityStatus.MarkCachingStarted(totalFileCount);
                cacheActivityStatus.UpdateProgress(processedFileCount, totalFileCount);

                foreach (BasicFileInfo imageFile in imageFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var cached = await TryGetAsync<ImageInfo>(ImageKey(imageFile.Id), ct);

                    if (cached != null && IsUnchanged(cached, imageFile))
                    {
                        imageCount++;
                    }
                    else
                    {
                        var imageInfo = ImageInfoHelpers.GetImageInfo(imageFile);

                        // All images cached even if they don't have geolocation data
                        if (imageInfo != null)
                        {
                            await cache.SetAsync(ImageKey(imageFile.Id), imageInfo, EntryOptions, cancellationToken: ct);
                            imageCacheNotifier.Publish(imageInfo);
                            imageCount++;
                            changedCount++;
                        }
                    }

                    processedFileCount++;
                    cacheActivityStatus.UpdateProgress(processedFileCount, totalFileCount);
                }

                var currentIds = imageFiles.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
                int removedCount = 0;
                foreach (var removed in previousFiles.Where(f => !currentIds.Contains(f.Id)))
                {
                    await cache.RemoveAsync(ImageKey(removed.Id), ct);
                    removedCount++;
                }

                Log.Information(
                    "Cache updated: {Count} images, {Changed} new or changed, {Removed} removed",
                    imageCount, changedCount, removedCount);

                await cache.SetAsync(CountKey, imageCount, EntryOptions, cancellationToken: ct);

                return imageCount;
            }
            finally
            {
                cacheActivityStatus.MarkCachingStopped();
                cacheSignal.Release();
            }
        }

        /// <inheritdoc />
        public Task<ImageInfo?> GetImageInfoAsync(string id, CancellationToken ct = default) =>
            TryGetAsync<ImageInfo>(ImageKey(id), ct);

        /// <inheritdoc />
        public Task<int> GetImageCountAsync(CancellationToken ct = default) =>
            TryGetAsync<int>(CountKey, ct);

        private static bool IsUnchanged(ImageInfo cached, BasicFileInfo current) =>
            cached.FilePath == current.FilePath
            && cached.LastModified == current.LastModified
            && cached.FileSize == current.FileSize;

        private async Task<T?> TryGetAsync<T>(string key, CancellationToken ct) =>
            await cache.GetOrCreateAsync<T?>(key, static _ => ValueTask.FromResult<T?>(default), LookupOptions, cancellationToken: ct);

        /// <summary>
        /// Resolves the configured folders and finds valid image files in them
        /// </summary>
        private List<BasicFileInfo> ScanImageFiles()
        {
            var imageFolders = ImageFetcherHelpers.ResolveImageFolders(config);
            if (imageFolders == null || imageFolders.Length == 0)
            {
                Log.Warning("No valid image folders found in configuration");
                return [];
            }

            Log.Information("ImageInfoFetcher initialized with ImageFolders: {@ImageFolders}", imageFolders);

            Log.Information("Resolving image files from configured folders...");

            List<BasicFileInfo> imageFiles = [.. ImageFetcherHelpers.GetImageList(imageFolders)
                .Select(path => (Path: path, Info: new FileInfo(path)))
                .Select(f => new BasicFileInfo(
                    ImageFetcherHelpers.GenerateIdForPath(f.Path),
                    Path.GetFileName(f.Path),
                    f.Path,
                    f.Info.LastWriteTimeUtc,
                    f.Info.CreationTimeUtc,
                    ImageFetcherHelpers.GetContentType(f.Path),
                    FileSize: f.Info.Length))
                .OrderByDescending(f => f.LastModified)
                ];

            Log.Information("ImageInfoFetcher found {ImageCount} image files in configured folders", imageFiles.Count);

            return imageFiles;
        }
    }
}
