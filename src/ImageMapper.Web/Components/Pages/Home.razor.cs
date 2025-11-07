using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.Web.Components.Pages
{
    public partial class Home
    {
        private IEnumerable<ImageInfo> images = [];
        private readonly CancellationTokenSource cts = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Any additional JS interop can be done here if needed
                images = await imageFetcher.Fetch(cts.Token);

                await JS.InvokeVoidAsync("initClusterMap",
                    cts.Token,
                    images.Select(i => new
                    {
                        i.FileName, i.Latitude, i.Longitude,
                        Url = $"/api/images/raw/{Uri.EscapeDataString(i.RelativePath)}"
                    }));
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}