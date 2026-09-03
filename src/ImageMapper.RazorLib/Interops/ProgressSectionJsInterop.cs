using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    internal sealed class ProgressSectionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Sections/ProgressSection.razor.js").AsTask());

        public async ValueTask SetProgressBarWidth(int percentage)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setProgressBarWidth", percentage);
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
