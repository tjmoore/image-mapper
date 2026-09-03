using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Provides JavaScript interop functionality for the ProgressSection component, allowing for communication between Blazor and JavaScript.
    /// This class can be registered as scoped DI service and then injected into Blazor components for use
    /// and is loaded on demand when first needed.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance used for invoking JavaScript functions.</param>
    internal sealed class ProgressSectionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Components/Sections/ProgressSection.razor.js").AsTask());

        /// <summary>
        /// Sets the width of the progress bar in the ProgressSection component by invoking the corresponding JavaScript function.
        /// </summary>
        /// <param name="percentage">The width of the progress bar as a percentage.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask SetProgressBarWidth(int percentage)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setProgressBarWidth", percentage);
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
