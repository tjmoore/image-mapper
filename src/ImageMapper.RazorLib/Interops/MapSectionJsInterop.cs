using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// JS interop functionality for the MapSection component in a Blazor application.
    /// This class can be registered as scoped DI service and then injected into Blazor components for use
    /// and is loaded on demand when first needed.
    /// </summary>
    /// <param name="jsRuntime"></param>
    internal sealed class MapSectionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Sections/MapSection.razor.js").AsTask());

        public async ValueTask InitClusterMap()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("initClusterMap");
        }

        public async ValueTask AdjustMapLayout()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("adjustMapLayout");
        }

        public async ValueTask AddMarkerToMap(ImageInfo image)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("addMarkerToMap", new ImageInfo(image, $"/api/images/raw/{image.Id}"));
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                try { await module.DisposeAsync(); } catch (JSDisconnectedException) { }
            }
        }
    }
}
