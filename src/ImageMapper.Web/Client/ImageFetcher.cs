using ImageMapper.Api.Services;
using ImageMapper.Models;

namespace ImageMapper.Web.Client
{
    /// <summary>
    /// Fetches images
    /// </summary>
    /// <param name="imageService">The image service to use for fetching images</param>
    public class ImageFetcher(IImageService imageService)
    {
        /// <summary>
        /// Fetch the total count of available images
        /// </summary>
        /// <returns>The total count of images</returns>
        public int FetchImageCount() => imageService.GetImageCount();

        /// <summary>
        /// Fetch list of available images with metadata, streamed as async enumerable
        /// </summary>
        /// <returns>An async enumerable of image information</returns>
        public IAsyncEnumerable<ImageInfo?> FetchImageList(CancellationToken ct = default)
        {
            return imageService.GetImagesAsync(ct: ct);
        }

        /// <summary>
        /// Fetch image content streamed to the caller without buffering the entire response in memory.
        /// Caller is responsible for disposing the returned Stream when finished. 
        /// </summary>
        /// <param name="id">The unique image ID</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Stream?> FetchRawImageStream(string id, CancellationToken ct)
        {
            var bytes = await imageService.GetImageBytesAsync(id, ct);
            if (bytes == null)
                return null;

            return new MemoryStream(bytes);
        }
    }
}
