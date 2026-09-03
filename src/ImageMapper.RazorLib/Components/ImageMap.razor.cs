using ImageMapper.Models;

namespace ImageMapper.RazorLib.Components
{
    public sealed partial class ImageMap
    {
        private readonly CancellationTokenSource _cts = new();
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
                _ = ConsumeCacheStatusStreamAsync(_cts.Token);

                // Fetch total image count
                _totalImages = ImageService.GetImageCount();

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
                    // Skips images that are null or have invalid data, or have no geolocation information as there's nothing to plot on the map
                    if (image == null || string.IsNullOrWhiteSpace(image.Id) || string.IsNullOrWhiteSpace(image.FileName) ||
                        image.Latitude == 0 || image.Longitude == 0)
                    {
                        _skippedImages++;
                        continue;
                    }

                    // Add marker to the map with existing image detail and URL to the raw image
                    await MapModule.AddMarkerToMap(image);

                    _imagesLoaded++;
                    batchCount++;

                    // Update progress bar
                    if (_totalImages > 0)
                    {
                        _progressPercentage = (int)((_imagesLoaded * 100) / _totalImages);
                        await ProgressModule.SetProgressBarWidth(_progressPercentage);
                    }

                    if (batchCount % batchSize == 0)
                    {
                        await InvokeAsync(StateHasChanged);
                    }
                }

                // Hide progress container when done
                _isProgressVisible = false;
                _isImageCountVisible = _imagesLoaded > 0;

                // Final update to ensure UI is synchronized
                await InvokeAsync(StateHasChanged);
                await MapModule.AdjustMapLayout();
            }
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
