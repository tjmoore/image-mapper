using ImageMapper.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace ImageMapper.Api.Services
{
    // Based on sample https://learn.microsoft.com/en-us/dotnet/core/extensions/caching#photo-service-scenario

    public sealed class ImageWorkerService(
        IConfiguration _config,
        CacheSignal<ImageInfo> _imageCacheSignal,
        IMemoryCache _cache) : BackgroundService
    {
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(3);

        private bool _isCacheInitialized = false;

        /// <summary>
        /// Start the worker service
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task StartAsync(CancellationToken ct)
        {
            await base.StartAsync(ct);
        }

        /// <summary>
        /// The main execution loop of the worker service. It updates the image cache at regular intervals and handles cancellation requests.
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Log.Information("Updating cache");

                var fetcher = new ImageInfoFetcher(_config);

                int totalImageCount = fetcher.GetImageCount();

                var cacheInfo = new ImageCacheInfo(totalImageCount, 0, []);
                _cache.Set("ImageCacheInfo", cacheInfo);

                try
                {
                    await _imageCacheSignal.WaitAsync(ct);

                    var keys = new HashSet<string>();

                    // Set each image info in the cache under its ID as key
                    await foreach (ImageInfo image in fetcher.GetAllAsync(ct))
                    {
                        _cache.Set(image.Id, image);
                        keys.Add(image.Id);
                    }

                    // Update cache info with the list of keys and count for easy retrieval
                    cacheInfo = new ImageCacheInfo(totalImageCount, keys.Count, keys);
                    _cache.Set("ImageCacheInfo", cacheInfo);

                    Log.Information("Cache updated with {Count} images", cacheInfo.TotalImageFiles);
                }
                finally
                {
                    if (!_isCacheInitialized)
                    {
                        _imageCacheSignal.Release();
                        _isCacheInitialized = true;
                    }
                }

                try
                {
                    Log.Information(
                        "Will attempt to update the cache in {Hours} hours from now",
                        _updateInterval.Hours);

                    await Task.Delay(_updateInterval, ct);
                }
                catch (OperationCanceledException)
                {
                    Log.Warning("Cancellation acknowledged: shutting down");
                    break;
                }
            }
        }
    }
}
