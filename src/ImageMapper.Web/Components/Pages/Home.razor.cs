using ImageMapper.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ImageMapper.Web.Components.Pages
{
    public partial class Home
    {
        private readonly CancellationTokenSource cts = new();
        private IJSObjectReference? mapSectionModule;
        private IJSObjectReference? imageCountSectionModule;
        private IJSObjectReference? progressSectionModule;
        private IJSObjectReference? imageModalSectionModule;
        private int totalImages = 0;
        private int skippedImages = 0;
        private int imagesLoaded = 0;
        private string cacheStatusText = "Checking...";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var mapImportTask = JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/MapSection.razor.js").AsTask();
                var imageCountImportTask = JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/ImageCountSection.razor.js").AsTask();
                var progressImportTask = JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/ProgressSection.razor.js").AsTask();
                var imageModalImportTask = JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/ImageModalSection.razor.js").AsTask();

                await Task.WhenAll(mapImportTask, imageCountImportTask, progressImportTask, imageModalImportTask);

                var mapModule = mapImportTask.Result;
                var imageCountModule = imageCountImportTask.Result;
                var progressModule = progressImportTask.Result;
                var imageModalModule = imageModalImportTask.Result;

                mapSectionModule = mapModule;
                imageCountSectionModule = imageCountModule;
                progressSectionModule = progressModule;
                imageModalSectionModule = imageModalModule;

                _ = ConsumeCacheStatusStreamAsync(cts.Token);

                // Fetch total image count
                totalImages = await imageFetcher.FetchImageCount(cts.Token);

                await imageModalModule.InvokeVoidAsync("setupImageModal");

                // Show progress container if there are images
                if (totalImages > 0)
                {
                    await progressModule.InvokeVoidAsync("showProgressContainer");
                }

                // Initialize the map with cluster grouping
                await mapModule.InvokeVoidAsync("initClusterMap");

                // Add markers as images arrive in batches for better performance
                int batchSize = 10;
                int batchCount = 0;
                await foreach (ImageInfo? image in imageFetcher.Fetch(cts.Token))
                {
                    // Skips images that are null or have invalid data, or have no geolocation information as there's nothing to plot on the map
                    if (image == null || string.IsNullOrWhiteSpace(image.Id) || string.IsNullOrWhiteSpace(image.FileName) ||
                        image.Latitude == 0 || image.Longitude == 0)
                    {
                        skippedImages++;
                        continue;
                    }

                    await mapModule.InvokeVoidAsync("addMarkerToMap",
                        new
                        {
                            image.FileName,
                            image.Latitude,
                            image.Longitude,
                            Url = $"/api/images/raw/{image.Id}"
                        });

                    imagesLoaded++;
                    batchCount++;

                    // Update progress bar
                    if (totalImages > 0)
                    {
                        int percentage = (int)((imagesLoaded * 100) / totalImages);
                        await progressModule.InvokeVoidAsync("updateProgress", imagesLoaded, totalImages, percentage);
                    }

                    if (batchCount % batchSize == 0)
                    {
                        StateHasChanged();
                    }
                }

                // Hide progress container when done
                await progressModule.InvokeVoidAsync("hideProgressContainer");

                // Update image count with skipped count if any
                await imageCountModule.InvokeVoidAsync("updateImageCount", imagesLoaded, skippedImages);

                // Final update to ensure UI is synchronized
                StateHasChanged();
            }
        }

        private async Task ConsumeCacheStatusStreamAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await foreach (var cacheStatus in imageFetcher.StreamCacheStatus(ct))
                    {
                        cacheStatusText = FormatCacheStatusText(cacheStatus);
                        await InvokeAsync(StateHasChanged);
                    }

                    cacheStatusText = "Unavailable";
                    await InvokeAsync(StateHasChanged);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (HttpRequestException)
                {
                    cacheStatusText = "Unavailable";
                    await InvokeAsync(StateHasChanged);
                }
                catch (JsonException)
                {
                    cacheStatusText = "Unavailable";
                    await InvokeAsync(StateHasChanged);
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
            cts.Cancel();
            cts.Dispose();

            await DisposeModuleAsync(mapSectionModule);
            await DisposeModuleAsync(imageCountSectionModule);
            await DisposeModuleAsync(progressSectionModule);
            await DisposeModuleAsync(imageModalSectionModule);
        }

        private static async ValueTask DisposeModuleAsync(IJSObjectReference? module)
        {
            if (module is null)
            {
                return;
            }

            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}