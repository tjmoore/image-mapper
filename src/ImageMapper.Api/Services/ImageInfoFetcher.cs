using ImageMapper.Models;
using Serilog;

namespace ImageMapper.Api.Services
{
    /// <summary>
    /// Fetches image information from configured folders and extracts their metadata, including geolocation if available
    /// </summary>
    /// <remarks>This class is not itself thread-safe</remarks>
    public class ImageInfoFetcher(IConfiguration _config)
    {
        private List<BasicFileInfo>? _imageFiles;

        private int _imageCount;

        private bool _initialised = false;

        /// <summary>
        /// Fetches a list of all image files from the configured folders
        /// </summary>
        /// <returns>An enumerable of image file paths</returns>
        public IEnumerable<BasicFileInfo> GetImageFiles()
        {
            CheckInitialise();

            return _imageFiles ?? Enumerable.Empty<BasicFileInfo>();
        }

        /// <summary>
        /// Gets the image information for a specific image file, including metadata and geolocation if available
        /// </summary>
        /// <param name="imageFile">The image file</param>
        /// <returns>The image information, or null if the image file is not found</returns>
        public ImageInfo? GetImageInfo(BasicFileInfo imageFile)
        {
            CheckInitialise();

            if (_imageFiles == null || !_imageFiles.Contains(imageFile))
                return null;

            return ImageInfoHelpers.GetImageInfo(imageFile);
        }

        /// <summary>
        /// Returns the total count of image files in the configured folders
        /// </summary>
        /// <returns>The total count of image files</returns>
        public int GetImageCount()
        {
            CheckInitialise();

            return _imageCount;
        }

        /// <summary>
        /// Clears the initialised flag and triggers reinitialisation of the fetcher
        /// </summary>
        public void Reinitialise()
        {
            _initialised = false;
        }

        /// <summary>
        /// Initialises the fetcher by resolving folders and finding valid files in those folders
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void CheckInitialise()
        {
            if (_initialised)
                return;

            var imageFolders = ImageFetcherHelpers.ResolveImageFolders(_config);
            if (imageFolders == null || imageFolders.Length == 0)
            {
                Log.Warning("No valid image folders found in configuration");
                return;
            }

            Log.Information("ImageInfoFetcher initialized with ImageFolders: {@ImageFolders}", imageFolders);

            _imageFiles = [.. ImageFetcherHelpers.GetImageList(imageFolders)
                .Select(f => new BasicFileInfo(ImageFetcherHelpers.GenerateIdForPath(f), Path.GetFileName(f), f))];

            _imageCount = _imageFiles.Count;

            Log.Information("ImageInfoFetcher found {ImageCount} image files in configured folders", _imageCount);

            _initialised = true;
        }
    }
}
