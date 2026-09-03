using ImageMapper.RazorLib.Components.Overlays;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Provides JavaScript interop functionality for the ImageModal component, allowing for communication between Blazor and JavaScript.
    /// This class can be registered as scoped DI service and then injected into Blazor components for use
    /// and is loaded on demand when first needed.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance used for invoking JavaScript functions.</param>
    internal sealed class ImageModalJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Overlays/ImageModal.razor.js").AsTask());

        /// <summary>
        /// Sets the DotNetObjectReference for the ImageModal component, allowing JavaScript to invoke .NET methods on the component.
        /// </summary>
        /// <param name="dotNetRef">The DotNetObjectReference for the ImageModal component.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask SetImageModalDotNetRef(DotNetObjectReference<ImageModal>? dotNetRef)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setImageModalDotNetRef", dotNetRef);
        }

        /// <summary>
        /// Sets up the key handler for the ImageModal component, allowing JavaScript to handle key events.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask SetupImageModalKeyHandler()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setupImageModalKeyHandler");
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
