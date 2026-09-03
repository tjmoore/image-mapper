using ImageMapper.RazorLib.Components.Overlays;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    internal sealed class ImageModalJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Overlays/ImageModal.razor.js").AsTask());

        public async ValueTask SetImageModalDotNetRef(DotNetObjectReference<ImageModal>? dotNetRef)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setImageModalDotNetRef", dotNetRef);
        }

        public async ValueTask SetupImageModalKeyHandler()
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setupImageModalKeyHandler");
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
