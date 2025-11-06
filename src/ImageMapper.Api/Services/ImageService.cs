using ImageMapper.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;

namespace ImageMapper.Api.Services;

public class ImageService : IImageService
{
    private readonly IConfiguration _config;
    private readonly string _imagesRoot;

    public ImageService(IConfiguration config)
    {
        _config = config;
        _imagesRoot = _config["ImageFolder"] ?? throw new InvalidOperationException("ImageFolder not configured");
    }

    public async Task<IEnumerable<ImageInfo>> GetImagesAsync(CancellationToken ct = default)
    {
        if (!System.IO.Directory.Exists(_imagesRoot))
            return [];

        // TODO: this will need to be optimised for large file stores and possibly return async enumerable

        var extensions = new[] { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".nef" };

        var files = System.IO.Directory.EnumerateFiles(_imagesRoot, "*.*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        var list = new List<ImageInfo>();
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
            list.Add(info);
        }

        return await Task.FromResult(list);
    }

    public async Task<byte[]?> GetImageBytesAsync(string relativePath, CancellationToken ct = default)
    {
        var full = Path.Combine(_imagesRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(full))
            return null;

        return await File.ReadAllBytesAsync(full, ct);
    }
}
