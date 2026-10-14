using ImageMagick;

namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Converts formats not displayable by browsers (HEIC/HEIF and camera RAW) to JPEG using Magick.NET.
    /// </summary>
    internal sealed class MagickImageConverter : IImageConverter
    {
        internal static readonly IReadOnlySet<string> Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".heic", ".heif",
            ".nef", ".crw", ".cr2", ".orf", ".arw", ".raf", ".srw", ".x3f", ".rw2", ".rwl", ".dcr", ".dng"
        };

        private static readonly HashSet<string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/heic", "image/heif",
            "image/x-nikon-nef", "image/x-canon-crw", "image/x-canon-cr2", "image/x-olympus-orf", "image/x-sony-arw",
            "image/x-fuji-raf", "image/x-samsung-srw", "image/x-sigma-x3f", "image/x-panasonic-raw",
            "image/x-leica-rwl", "image/x-kodak-dcr", "image/x-adobe-dng"
        };

        public bool CanConvert(string? contentType, string? fileName) =>
            (contentType != null && ContentTypes.Contains(contentType))
            || (!string.IsNullOrEmpty(fileName) && Extensions.Contains(Path.GetExtension(fileName)));

        public async Task<Stream> ConvertToJpegAsync(Stream source, CancellationToken cancellationToken = default)
        {
            using var image = new MagickImage();
            await image.ReadAsync(source, cancellationToken);
            image.AutoOrient();
            image.Format = MagickFormat.Jpeg;

            var output = new MemoryStream();
            try
            {
                await image.WriteAsync(output, cancellationToken);
                output.Position = 0;
                return output;
            }
            catch
            {
                await output.DisposeAsync();
                throw;
            }
        }
    }
}
