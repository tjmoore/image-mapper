using ImageMapper.Models;

namespace ImageMapper.Api.Services;

public interface IImageService
{
    IAsyncEnumerable<ImageInfo> GetImagesAsync(CancellationToken ct = default);
    Task<byte[]?> GetImageBytesAsync(string relativePath, CancellationToken ct = default);
}
