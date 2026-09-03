using ImageMapper.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ImageMapper.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController(IImageService imageService) : ControllerBase
    {
        [HttpGet("raw/{id}")]
        public async Task<IActionResult> GetRaw(string id, CancellationToken ct)
        {
            Log.Debug("GET /api/images/raw/{Id} - Retrieving image", id);

            var bytes = await imageService.GetImageBytesAsync(id, ct);
            if (bytes == null)
                return NotFound();

            // Return the stream directly; MVC will handle disposing it when the response is complete.
            return File(new MemoryStream(bytes), "application/octet-stream");
        }
    }
}
