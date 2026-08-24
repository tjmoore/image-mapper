using ImageMapper.Web.Client;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ImageMapper.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController(ImageFetcher imageFetcher) : ControllerBase
    {
        [HttpGet("raw/{id}")]
        public async Task<IActionResult> GetRaw(string id, CancellationToken ct)
        {
            Log.Debug("GET /api/images/raw/{Id} - Retrieving image", id);

            var stream = await imageFetcher.FetchRawImageStream(id, ct);
            if (stream == null)
                return NotFound();

            // Return the stream directly; MVC will handle disposing it when the response is complete.
            return File(stream, "application/octet-stream");
        }
    }
}
