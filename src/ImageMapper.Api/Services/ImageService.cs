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
    private readonly string[] _imageFolders;

    // In-memory lookup from ID to full file path
    private static readonly Dictionary<string, string> IdToPathMapping = [];
    private static readonly SemaphoreSlim MappingSem = new(1, 1);

    private static readonly string[] ValidExtensions = [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic", ".heif", ".ico", ".webp", ".pcx",".tif", ".tiff",
        ".nef", ".crw", ".cr2", ".orf", ".arw", ".raf", ".srw", ".x3f", ".rw2", ".rwl", ".dcr", ".dng"        
    ];

    public ImageService(IConfiguration config)
    {
        _config = config;

        string? imageFolder = _config["ImageFolder"];
        if (string.IsNullOrEmpty(imageFolder))
        {
            string[]? imageFolders = _config.GetSection("ImageFolders").Get<string[]>();

            if (imageFolders == null || imageFolders.Length == 0)
                throw new InvalidOperationException("Either ImageFolder or ImageFolders must be configured");

            _imageFolders = imageFolders;
        }
        else
        {
            _imageFolders = [imageFolder];
        }
        
        Log.Information("ImageService initialized with ImageFolders: {@ImageFolders}", _imageFolders);
    }

    public async IAsyncEnumerable<ImageInfo> GetImagesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        if (_imageFolders == null || _imageFolders.Length == 0)
            yield break;

        var files = _imageFolders
            .Where(folder => System.IO.Directory.Exists(folder))
            .SelectMany(folder => System.IO.Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
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
        if (_imageFolders == null || _imageFolders.Length == 0)
            return Task.FromResult(0);

        var count = _imageFolders
            .Where(folder => System.IO.Directory.Exists(folder))
            .SelectMany(folder => System.IO.Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            .Count(f => ValidExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        return Task.FromResult(count);
    }
}
