using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.Web.Components.Pages
{
    public partial class Home
    {
        private readonly CancellationTokenSource cts = new();
        private int totalImages = 0;
        private int skippedImages = 0;
        private int imagesLoaded = 0;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Fetch total image count
                totalImages = await imageFetcher.FetchImageCount(cts.Token);

                // Show progress container if there are images
                if (totalImages > 0)
                {
                    await JS.InvokeVoidAsync("showProgressContainer");
                }

                // Initialize the map with cluster grouping
                await JS.InvokeVoidAsync("initClusterMap", cts.Token);

                // Add markers as images arrive in batches for better performance
                int batchSize = 10;
                int batchCount = 0;
                await foreach (ImageInfo? image in imageFetcher.Fetch(cts.Token))
                {
                    if (image == null || image.Longitude == null || image.Latitude == null)
                    {
                        skippedImages++;
                        continue;
                    }

                    await JS.InvokeVoidAsync("addMarkerToMap",
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
                        await JS.InvokeVoidAsync("updateProgress", imagesLoaded, totalImages, percentage);
                    }

                    if (batchCount % batchSize == 0)
                    {
                        StateHasChanged();
                    }
                }

                // Hide progress container when done
                await JS.InvokeVoidAsync("hideProgressContainer");

                // Update image count with skipped count if any
                await JS.InvokeVoidAsync("updateImageCount", imagesLoaded, skippedImages);

                // Final update to ensure UI is synchronized
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}