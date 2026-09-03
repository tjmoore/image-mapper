using ImageMapper.Models;
using Microsoft.JSInterop;

namespace ImageMapper.RazorLib.Components.Overlays
{
    public sealed partial class ImageModal
    {
        private bool _isVisible;
        private bool _infoPanelOpen;
        private string _imageSrc = string.Empty;
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
        public Task ShowImage(ImageInfo imageInfo)
        {
            _imageSrc = imageInfo.Url;
            _currentImageInfo = imageInfo;
            _isVisible = true;
            return InvokeAsync(StateHasChanged);
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
