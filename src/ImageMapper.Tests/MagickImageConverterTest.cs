using ImageMagick;
using ImageMapper.RazorLib.Interops;

namespace ImageMapper.Tests
{
    [TestFixture]
    public class MagickImageConverterTest
    {
        [TestCase("image/heic", "a.bin", true)]
        [TestCase("image/heif", null, true)]
        [TestCase(null, "photo.HEIC", true)]
        [TestCase("application/octet-stream", "photo.heif", true)]
        [TestCase("image/x-canon-cr2", "photo.cr2", true)]
        [TestCase("image/x-adobe-dng", null, true)]
        [TestCase(null, "photo.NEF", true)]
        [TestCase("image/jpeg", "photo.jpg", false)]
        [TestCase("image/png", "photo.png", false)]
        [TestCase("image/tiff", "photo.tif", false)]
        public void CanConvertMatchesConvertibleFormatsOnly(string? contentType, string? fileName, bool expected)
        {
            var converter = new MagickImageConverter();

            Assert.That(converter.CanConvert(contentType, fileName), Is.EqualTo(expected));
        }

        [Test]
        public void AllConvertibleExtensionsAreReadableByMagick()
        {
            var readable = MagickNET.SupportedFormats
                .Where(f => f.SupportsReading)
                .Select(f => "." + f.Format.ToString().ToLowerInvariant())
                .ToHashSet();

            var unsupported = MagickImageConverter.Extensions.Where(e => !readable.Contains(e)).ToList();

            Assert.That(unsupported, Is.Empty);
        }
    }
}
