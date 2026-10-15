using ImageMapper.Services.Models;

namespace ImageMapper.RazorLib.Components
{
    public sealed partial class ImageMap
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly HashSet<string> _seenImageIds = [];
        private int _totalImages = 0;
        private int _skippedImages = 0;
        private int _imagesLoaded = 0;
        private int _progressPercentage = 0;
        private bool _isProgressVisible = false;
        private bool _isImageCountVisible = false;
        private string _cacheStatusText = "Checking...";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Subscribe before the snapshot so no image cached in between is missed; duplicates are filtered by ID
                var newImages = ImageService.SubscribeToNewImagesAsync(_cts.Token);

                _ = ConsumeCacheStatusStreamAsync(_cts.Token);

                // Fetch total image count
                _totalImages = ImageService.GetImageCount();
                if (_totalImages == 0)
                {
                    _totalImages = CacheStatus.GetStatus().TotalFileCount;
                }

                // Show progress container if there are images
                if (_totalImages > 0)
                {
                    _isProgressVisible = true;
                    _progressPercentage = 0;
                    await InvokeAsync(StateHasChanged);
                }

                // Initialize the map with cluster grouping
                await MapModule.InitClusterMap();

                if (_isProgressVisible)
                {
                    await ProgressModule.SetProgressBarWidth(_progressPercentage);
                    await MapModule.AdjustMapLayout();
                }

                // Add markers as images arrive in batches for better performance
                int batchSize = 10;
                int batchCount = 0;
                await foreach (ImageInfo? image in ImageService.GetImagesAsync(_cts.Token))
                {
                    if (await TryAddImageAsync(image) && ++batchCount % batchSize == 0)
                    {
                        await InvokeAsync(StateHasChanged);
                    }
                }

                _isProgressVisible = false;
                _isImageCountVisible = _imagesLoaded > 0;
                await InvokeAsync(StateHasChanged);
                await MapModule.AdjustMapLayout();

                await ConsumeNewImagesAsync(newImages, _cts.Token);
            }
        }

        private async Task ConsumeNewImagesAsync(IAsyncEnumerable<ImageInfo> newImages, CancellationToken ct)
        {
            int pending = 0;
            try
            {
                await foreach (ImageInfo image in newImages.WithCancellation(ct))
                {
                    if (await TryAddImageAsync(image))
                    {
                        _isImageCountVisible = true;
                        pending++;
                    }

                    // Throttle re-renders while images stream in
                    if (pending >= 25)
                    {
                        pending = 0;
                        await InvokeAsync(StateHasChanged);
                    }
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
        }

        /// <summary>
        /// Plots the image if it is new and has valid geolocation data
        /// </summary>
        /// <returns>True if a marker was added</returns>
        private async Task<bool> TryAddImageAsync(ImageInfo? image)
        {
            if (image == null || string.IsNullOrWhiteSpace(image.Id) || !_seenImageIds.Add(image.Id))
            {
                return false;
            }

            // Skips images with invalid data or no geolocation information as there's nothing to plot on the map
            if (string.IsNullOrWhiteSpace(image.FileName) || image.Latitude == 0 || image.Longitude == 0)
            {
                _skippedImages++;
                return false;
            }

            await MapModule.AddMarkerToMap(image);
            _imagesLoaded++;

            if (_isProgressVisible && _totalImages > 0)
            {
                _progressPercentage = (int)((_imagesLoaded * 100) / _totalImages);
                await ProgressModule.SetProgressBarWidth(_progressPercentage);
            }

            return true;
        }

        private async Task ConsumeCacheStatusStreamAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await foreach (var status in CacheStatus.StreamStatuses(ct))
                    {
                        _cacheStatusText = FormatCacheStatusText(status);
                        await InvokeAsync(StateHasChanged);
                    }

                    _cacheStatusText = "Unavailable";
                    await InvokeAsync(StateHasChanged);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private static string FormatCacheStatusText(CacheStatusInfo? cacheStatus)
        {
            if (cacheStatus == null)
            {
                return "Unavailable";
            }

            if (!cacheStatus.IsCaching)
            {
                return "Idle";
            }

            if (cacheStatus.TotalFileCount <= 0)
            {
                return "Processing images...";
            }

            var processedCount = Math.Min(cacheStatus.ProcessedFileCount, cacheStatus.TotalFileCount);
            return $"Processing images... ({processedCount}/{cacheStatus.TotalFileCount})";
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
