using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Components.Sections
{
    public sealed partial class MapSection
    {
        private DotNetObjectReference<MapSection>? _dotNetRef;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await MapModule.SetMapSectionDotNetRef(_dotNetRef);
            }
        }

        /// <summary>
        /// Invoked from JavaScript when a map popup is opened, so the popup's image element
        /// can be populated by streaming the image data instead of loading it from a URL.
        /// </summary>
        /// <param name="imageInfo">The image to stream into the popup's image element.</param>
        /// <param name="elementId">The id of the popup's image element to populate.</param>
        [JSInvokable]
        public Task PopulatePopupImage(ImageInfo imageInfo, string elementId)
        {
            return ImageSourceModule.SetImageSource(imageInfo, elementId).AsTask();
        }
    }
}
