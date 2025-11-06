using ImageMapper.Api.Services;
using ImageMapper.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ImageMapper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController(IImageService svc) : ControllerBase
{
    private readonly IImageService _svc = svc;

    [HttpGet]
    public async Task<IEnumerable<ImageInfo>> Get(CancellationToken ct)
    {
        var images = await _svc.GetImagesAsync(ct);

        Log.Debug("GET /api/images - Retrieved {Count} images", images.Count());

        return images;
    }

    [HttpGet("raw/{**relativePath}")]
    public async Task<IActionResult> GetRaw(string relativePath, CancellationToken ct)
    {
        Log.Debug("GET /api/images/raw/{RelativePath} - Retrieving image", relativePath);

        var bytes = await _svc.GetImageBytesAsync(relativePath, ct);
        if (bytes == null)
            return NotFound();

        return File(bytes, "application/octet-stream");
    }
}
