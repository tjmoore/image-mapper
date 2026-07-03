using ImageMapper.Models;
using Serilog;
using System.Runtime.CompilerServices;

namespace ImageMapper.Api.Services
{
    /// <summary>
    /// Fetches image information from configured folders and extracts their metadata, including geolocation if available
    /// </summary>
    public class ImageInfoFetcher(IConfiguration _config)
    {
        private List<string>? _imageFiles;

        private bool _initialised = false;

        /// <summary>
        /// Fetches all image info from the configured folders and extracts their metadata, including geolocation if available
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None</param>
        /// <returns></returns>
        /// <remarks>This method uses asynchronous streaming to yield image information as it is processed</remarks>
        public async IAsyncEnumerable<ImageInfo> GetAllAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            if (!_initialised)
                Initialise();

            if (_imageFiles == null || _imageFiles.Count == 0)
                yield break;

            foreach (string f in _imageFiles)
            {
                ct.ThrowIfCancellationRequested();

                yield return ImageInfoHelpers.GetImageInfo(f);
            }
        }

        /// <summary>
        /// Returns the total count of image files in the configured folders
        /// </summary>
        /// <returns>The total count of image files</returns>
        public int GetImageCount()
        {
            if (!_initialised)
                Initialise();

            return _imageFiles?.Count ?? 0;
        }

        /// <summary>
        /// Initialises the fetcher by resolving folders and finding valid files in those folders
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void Initialise()
        {
            var imageFolders = ImageFetcherHelpers.ResolveImageFolders(_config);
            if (imageFolders == null || imageFolders.Length == 0)
                throw new InvalidOperationException("ImageFolders must be configured with at least one folder");

            Log.Information("ImageInfoFetcher initialized with ImageFolders: {@ImageFolders}", imageFolders);

            _imageFiles = [.. ImageFetcherHelpers.GetImageList(imageFolders)];

            Log.Information("ImageInfoFetcher found {ImageCount} image files in configured folders", _imageFiles.Count);

            _initialised = true;
        }
    }
}
