using System.Buffers.Text;

namespace ImageMapper.Api.Services
{
    internal static class ImageFetcherHelpers
    {
        private static readonly string[] ValidExtensions = [
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic", ".heif", ".ico", ".webp", ".pcx",".tif", ".tiff",
                                ".nef", ".crw", ".cr2", ".orf", ".arw", ".raf", ".srw", ".x3f", ".rw2", ".rwl", ".dcr", ".dng"
        ];

        /// <summary>
        /// Asynchronously retrieves the image data as a byte array from the specified file path
        /// </summary>
        /// <param name="filepath">The file path of the image</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a byte array of the image data, or
        /// null if the image could not be found.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
        public static async Task<byte[]?> GetImageBytesAsync(string filepath, CancellationToken ct = default)
        {
            if (File.Exists(filepath))
            {
                return await File.ReadAllBytesAsync(filepath, ct);
            }

            return null;
        }

        /// <summary>
        /// Gets a list of image files from the specified folders, filtering by valid image extensions
        /// </summary>
        /// <param name="folders">An array of folder paths to search for image files</param>
        /// <returns>An enumerable of image file paths</returns>
        public static IEnumerable<string> GetImageList(string[] folders)
        {
            return folders
                .Where(folder => Directory.Exists(folder))
                .SelectMany(folder => Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
                .Where(f => ValidExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }

        /// <summary>
        /// // Generate a unique ID based on the full path
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string GenerateIdForPath(string filepath) =>
            Base64Url.EncodeToString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(filepath)));

        /// <summary>
        /// Returns a list of valid image folders
        /// </summary>
        /// <param name="config">The configuration object</param>
        /// <returns>An array of valid image folder paths, or null if none are found</returns>
        /// <remarks>IGNORE folders are ignored. This is primarily used for unit tests in development environments where configs may be built from
        /// appsettings and then overridden by environment variables in tests</remarks>
        public static string[]? ResolveImageFolders(IConfiguration config) => config.GetSection("ImageFolders")
            .Get<string[]>()?
            .Where(folder => !string.IsNullOrWhiteSpace(folder) && folder != "IGNORE")
            .Where(folder => Directory.Exists(folder))
            .ToArray();
    }
}