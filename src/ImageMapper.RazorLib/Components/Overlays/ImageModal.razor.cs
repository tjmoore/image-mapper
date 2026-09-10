using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Components.Overlays
{
    public sealed partial class ImageModal
    {
        private const string ImageElementId = "image-modal-full-image";

        private bool _isVisible;
        private bool _infoPanelOpen;
        private ImageInfo? _currentImageInfo;
        private DotNetObjectReference<ImageModal>? _dotNetRef;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await ImageModalModule.SetImageModalDotNetRef(_dotNetRef);
                await ImageModalModule.SetupImageModalKeyHandler();
            }
        }

        [JSInvokable]
        public async Task ShowImage(ImageInfo imageInfo)
        {
            _currentImageInfo = imageInfo;
            _isVisible = true;
            await InvokeAsync(StateHasChanged);
            await ImageSourceModule.SetImageSource(imageInfo, ImageElementId);
        }

        [JSInvokable]
        public Task CloseModal()
        {
            _isVisible = false;
            return InvokeAsync(StateHasChanged);
        }

        private void OnBackdropClick()
        {
            _isVisible = false;
        }

        private void ToggleInfoPanel()
        {
            _infoPanelOpen = !_infoPanelOpen;
        }
    }
}
