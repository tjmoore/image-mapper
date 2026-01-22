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
                // Initialize the map once
                await JS.InvokeVoidAsync("initClusterMap", cts.Token);

                // Add markers as images arrive
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
                    
                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}