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
    public IAsyncEnumerable<ImageInfo> Get(CancellationToken ct)
    {
        return _svc.GetImagesAsync(ct);
    }

    [HttpGet("raw/{**relativePath}")]
    public async Task<IActionResult> GetRaw(string relativePath, CancellationToken ct)
    {
        Log.Debug("GET /api/images/raw/{RelativePath} - Retrieving image", relativePath);

        try
        {
            var bytes = await _svc.GetImageBytesAsync(relativePath, ct);
            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream");
        }
        catch (ArgumentException ex)
        {
            Log.Warning("Path traversal rejected: {Message}", ex.Message);
            return BadRequest("Invalid path");
        }
    }
}
