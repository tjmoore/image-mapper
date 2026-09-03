using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Provides JavaScript interop functionality for the MapSection component, allowing for communication between Blazor and JavaScript.
    /// This class can be registered as scoped DI service and then injected into Blazor components for use
    /// and is loaded on demand when first needed.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance used for invoking JavaScript functions.</param>
    internal sealed class MapSectionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Sections/MapSection.razor.js").AsTask());

        /// <summary>
        /// Initializes the cluster map by invoking the corresponding JavaScript function.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask InitClusterMap()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("initClusterMap");
        }
        
        /// <summary>
        /// Adjusts the layout of the map by invoking the corresponding JavaScript function.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask AdjustMapLayout()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("adjustMapLayout");
        }

        /// <summary>
        /// Adds a marker to the map for the specified image by invoking the corresponding JavaScript function.
        /// </summary>
        /// <param name="image">The image information for which to add a marker.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask AddMarkerToMap(ImageInfo image)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("addMarkerToMap", new ImageInfo(image, $"/api/images/raw/{image.Id}"));
        }

        /// <summary>
        /// Disposes the JavaScript module when it is no longer needed.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
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
