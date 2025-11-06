using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.Web.Components.Pages
{
    public partial class Home
    {
        private IEnumerable<ImageInfo> images = [];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Any additional JS interop can be done here if needed
                images = await imageFetcher.Fetch();

                await JS.InvokeVoidAsync("initClusterMap",
                    images.Select(i => new
                    {
                        i.FileName, i.Latitude, i.Longitude,
                        Url = $"/api/images/raw/{Uri.EscapeDataString(i.RelativePath)}"
                    }));
            }
        }
    }
}