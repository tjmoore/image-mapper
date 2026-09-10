using Microsoft.Extensions.Configuration;
using System.Buffers.Text;

namespace ImageMapper.Services
{
    internal static class ImageFetcherHelpers
    {
        private static readonly string[] ValidExtensions = [
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic", ".heif", ".ico", ".webp", ".pcx",".tif", ".tiff",
                                ".nef", ".crw", ".cr2", ".orf", ".arw", ".raf", ".srw", ".x3f", ".rw2", ".rwl", ".dcr", ".dng"
        ];

        /// <summary>
        /// Gets a read-only stream for the specified image file path. The caller is responsible for disposing the stream.
        /// </summary>
        /// <param name="filepath">The file path of the image</param>
        /// <returns>A read-only stream of the image file, or null if the file does not exist</returns>
        public static Stream? GetImageStream(string filepath)
        {
            if (File.Exists(filepath))
            {
                return File.OpenRead(filepath);
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

        /// <summary>
        /// Returns the content type (MIME type) for the specified file path based on its extension
        /// </summary>
        /// <param name="filePath">The file path of the image</param>
        /// <returns>The content type (MIME type) of the image</returns>
        public static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".heic" or ".heif" => "image/heic",
                ".ico" => "image/x-icon",
                ".webp" => "image/webp",
                ".pcx" => "image/pcx",
                ".tif" or ".tiff" => "image/tiff",
                _ => "application/octet-stream", // Default for unknown types
            };
        }
    }
}