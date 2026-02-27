using ImageMapper.Api.Exceptions;
using ImageMapper.Api.Services;
using ImageMapper.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Web;

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

    [HttpGet("raw/{**relativePath}")]
    public async Task<IActionResult> GetRaw(string relativePath, CancellationToken ct)
    {
        // URL-decode the path to handle encoded path separators (%2F -> /)
        var decodedPath = HttpUtility.UrlDecode(relativePath);

        Log.Debug("GET /api/images/raw/{RelativePath} - Retrieving image", decodedPath);

        try
        {
            var bytes = await _svc.GetImageBytesAsync(decodedPath, ct);
            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream");
        }
        catch (PathTraversalException ex)
        {
            Log.Warning("Path traversal rejected: {Message}", ex.Message);
            return BadRequest("Invalid path");
        }
    }
}
