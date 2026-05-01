using ImageMapper.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace ImageMapper.Api.Services;

public class ImageService : IImageService
{
    private readonly IConfiguration _config;
    private readonly string _imagesRoot;

    // In-memory lookup from ID to full file path
    private static readonly Dictionary<string, string> IdToPathMapping = [];
    private static readonly SemaphoreSlim MappingSem = new(1, 1);

    private static readonly string[] ValidExtensions = [".jpg", ".jpeg", ".png", ".tif", ".tiff", ".nef"];

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

        var files = System.IO.Directory.EnumerateFiles(_imagesRoot, "*.*", SearchOption.AllDirectories)
            .Where(f => ValidExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        foreach (string f in files)
        {
            ct.ThrowIfCancellationRequested();
            var id = await GenerateIdForPath(f, ct);
            var info = new ImageInfo { Id = id, FileName = Path.GetFileName(f) };
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

    private static async Task<string> GenerateIdForPath(string fullPath, CancellationToken ct = default)
    {
        // Generate a unique ID based on the full path
        var id = Base64Url.EncodeToString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(fullPath)));

        await MappingSem.WaitAsync(ct);
        try
        {
            IdToPathMapping[id] = fullPath;
        }
        finally
        {
            MappingSem.Release();
        }

        return id;
    }

    public async Task<byte[]?> GetImageBytesAsync(string id, CancellationToken ct = default)
    {
        await MappingSem.WaitAsync(ct);
        try
        {
            if (!IdToPathMapping.TryGetValue(id, out var fullPath))
                return null;

            if (!File.Exists(fullPath))
            {
                // Remove stale mapping
                IdToPathMapping.Remove(id);
                return null;
            }

            return await File.ReadAllBytesAsync(fullPath, ct);
        }
        finally
        {
            MappingSem.Release();
        }
    }

    public Task<int> GetImageCountAsync(CancellationToken ct = default)
    {
        if (!System.IO.Directory.Exists(_imagesRoot))
            return Task.FromResult(0);

        var count = System.IO.Directory.EnumerateFiles(_imagesRoot, "*.*", SearchOption.AllDirectories)
            .Count(f => ValidExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        return Task.FromResult(count);
    }
}
