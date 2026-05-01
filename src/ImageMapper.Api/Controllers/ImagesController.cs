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

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(CancellationToken ct)
    {
        var count = await _svc.GetImageCountAsync(ct);
        return Ok(count);
    }

    [HttpGet("raw/{id}")]
    public async Task<IActionResult> GetRaw(string id, CancellationToken ct)
    {
        Log.Debug("GET /api/images/raw/{Id} - Retrieving image", id);

        var bytes = await _svc.GetImageBytesAsync(id, ct);
        if (bytes == null)
            return NotFound();

        return File(bytes, "application/octet-stream");
    }
}
