using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.Web.Components.Pages
{
    public partial class Home
    {
        private readonly CancellationTokenSource cts = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Initialize the map with cluster grouping
                await JS.InvokeVoidAsync("initClusterMap", cts.Token);

                // Add markers as images arrive in batches for better performance
                int batchSize = 10;
                int batchCount = 0;
                await foreach (ImageInfo? image in imageFetcher.Fetch(cts.Token))
                {
                    if (image == null)
                        continue;

                    await JS.InvokeVoidAsync("addMarkerToMap",
                        new
                        {
                            image.FileName,
                            image.Latitude,
                            image.Longitude,
                            Url = $"/api/images/raw/{Uri.EscapeDataString(image.RelativePath)}"
                        });

                    batchCount++;
                    if (batchCount % batchSize == 0)
                    {
                        StateHasChanged();
                    }
                }

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