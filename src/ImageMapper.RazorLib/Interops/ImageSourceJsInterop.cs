using ImageMapper.Services.Models;
using ImageMapper.Services;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Provides JavaScript interop functionality for setting image sources in Blazor components, allowing for communication between Blazor and JavaScript.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance used for invoking JavaScript functions.</param>
    /// <param name="imageService">The image service used for retrieving image streams.</param>
    internal sealed class ImageSourceJsInterop(IJSRuntime jsRuntime, IImageService imageService) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", $"{Constants.LibContentBase}/Interops/ImageSource.js").AsTask());

        /// <summary>
        /// Sets the image source for a specified HTML element by invoking a JavaScript function.
        /// This method retrieves the image stream from the image service and passes it to the JavaScript function along with the element ID, content type, and file name.
        /// </summary>
        /// <param name="imageInfo">The information about the image to be displayed.</param>
        /// <param name="elementId">The ID of the HTML element for which to set the image source.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask SetImageSource(ImageInfo imageInfo, string elementId)
        {
            var imageStream = imageService.GetImageStream(imageInfo.Id);
            if (imageStream == null)
                return;

            var module = await _moduleTask.Value;
            var streamRef = new DotNetStreamReference(imageStream);
            await module.InvokeVoidAsync("setImageSource", elementId, streamRef, imageInfo.ContentType, imageInfo.FileName);
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