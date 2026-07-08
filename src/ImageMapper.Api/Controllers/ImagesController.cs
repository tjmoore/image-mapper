using ImageMapper.Api.Services;
using ImageMapper.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

namespace ImageMapper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController(IImageService svc, CacheActivityStatus cacheActivityStatus) : ControllerBase
{
    private readonly IImageService _svc = svc;
    private readonly CacheActivityStatus _cacheActivityStatus = cacheActivityStatus;

    [HttpGet]
    public IAsyncEnumerable<ImageInfo> Get(CancellationToken ct)
    {
        return _svc.GetImagesAsync(ct: ct);
    }

    [HttpGet("count")]
    public Task<int> GetCount()
    {
        var count = _svc.GetImageCount();
        return Task.FromResult(count);
    }

    [HttpGet("cache-status")]
    public Task<CacheStatusInfo> GetCacheStatus()
    {
        return Task.FromResult(_cacheActivityStatus.GetStatus());
    }

    [HttpGet("cache-status/events")]
    public async Task GetCacheStatusEvents(CancellationToken ct)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await foreach (var status in _cacheActivityStatus.StreamStatuses(ct))
        {
            var payload = JsonSerializer.Serialize(status);
            await Response.WriteAsync($"event: cache-status\ndata: {payload}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
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
