using ImageMapper.Web.Client;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace ImageMapper.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController(ImageItemFetcher imageFetcher) : ControllerBase
    {
        [HttpGet("raw/{**relativePath}")]
        public async Task<IActionResult> GetRaw(string relativePath, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return BadRequest("Relative path cannot be empty.");

            // URL-decode the path to handle encoded path separators (%2F -> /)
            var decodedPath = HttpUtility.UrlDecode(relativePath);
            var stream = await imageFetcher.FetchRawImageStream(decodedPath, ct);
            if (stream == null)
                return NotFound();

            // Return the stream directly; MVC will handle disposing it when the response is complete.
            return File(stream, "application/octet-stream");
        }
    }
}
