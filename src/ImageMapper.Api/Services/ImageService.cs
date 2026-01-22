using ImageMapper.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;
using System.Runtime.CompilerServices;

namespace ImageMapper.Api.Services;

public class ImageService : IImageService
{
    private readonly IConfiguration _config;
    private readonly string _imagesRoot;

    public ImageService(IConfiguration config)
    {
        _config = config;
        _imagesRoot = _config["ImageFolder"] ?? throw new InvalidOperationException("ImageFolder not configured");

        Log.Information("ImageService initialized with ImageFolder: {ImageFolder}", _imagesRoot);
    }

    public async IAsyncEnumerable<ImageInfo> GetImagesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!System.IO.Directory.Exists(_imagesRoot))
            yield break;

        var extensions = new[] { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".nef" };

        var files = System.IO.Directory.EnumerateFiles(_imagesRoot, "*.*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        foreach (string f in files)
        {
            ct.ThrowIfCancellationRequested();
            var rel = Path.GetRelativePath(_imagesRoot, f).Replace("\\", "/");
            var info = new ImageInfo { RelativePath = rel, FileName = Path.GetFileName(f) };
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(f);
                var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
                if (gps != null)
                {
                    if (gps.TryGetGeoLocation(out GeoLocation location))
                    {
                        info.Latitude = location.Latitude;
                        info.Longitude = location.Longitude;
                    }
                    else
                    {
                        Log.Debug("No geolocation found in GPS data for file: {File}", f);
                    }
                }
            }
            catch
            {
                Log.Warning("Failed to read metadata for file: {File}", f);
            }
            yield return info;
        }
    }

    public async Task<byte[]?> GetImageBytesAsync(string relativePath, CancellationToken ct = default)
    {
        // Normalize path separators to OS-specific
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var full = Path.Combine(_imagesRoot, normalized);
        var fullPath = Path.GetFullPath(full);
        var rootPath = Path.GetFullPath(_imagesRoot);
        
        // Ensure rootPath ends with separator for proper boundary checking
        if (!rootPath.EndsWith(Path.DirectorySeparatorChar))
            rootPath += Path.DirectorySeparatorChar;
        
        // Validate that the resolved path is within the root directory
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path traversal detected", nameof(relativePath));
        }

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }
}
