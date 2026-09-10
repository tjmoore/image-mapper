using ImageMapper.Models;
using ImageMapper.Services;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Interops
{
    internal class ImageSourceJsInterop(IJSRuntime jsRuntime, IImageService imageService) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ImageMapper.RazorLib/Interops/ImageSource.js").AsTask());

        public async ValueTask SetImageSource(ImageInfo imageInfo, string elementId)
        {
            var imageStream = imageService.GetImageStream(imageInfo.Id);
            if (imageStream == null)
                return;

            var module = await _moduleTask.Value;
            var streamRef = new DotNetStreamReference(imageStream);
            await module.InvokeVoidAsync("setImageSource", elementId, streamRef, imageInfo.ContentType, imageInfo.FileName);
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