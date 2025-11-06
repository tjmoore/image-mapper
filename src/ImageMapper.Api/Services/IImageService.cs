using ImageMapper.Models;

namespace ImageMapper.Api.Services;

public interface IImageService
{
    Task<IEnumerable<ImageInfo>> GetImagesAsync(CancellationToken ct = default);
    Task<byte[]?> GetImageBytesAsync(string relativePath, CancellationToken ct = default);
}
