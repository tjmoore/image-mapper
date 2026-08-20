using ImageMapper.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Heif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using Serilog;

namespace ImageMapper.Api.Services
{
    internal static class ImageInfoHelpers
    {
        private struct Dimensions
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }       

        /// <summary>
        /// Gets image information including ID, filename, latitude, and longitude from the specified file path.
        /// </summary>
        /// <param name="imageFile">The image file</param>
        /// <returns>An ImageInfo object containing the image's ID, filename, latitude, and longitude, or null if the file does not exist</returns>
        public static ImageInfo? GetImageInfo(BasicFileInfo imageFile)
        {
            // TODO: potential for async operation if reading metadata is slow, but MetadataExtractor does not provide async methods

            if (!File.Exists(imageFile.FilePath))
            {
                Log.Warning("File does not exist: {File}", imageFile.FilePath);
                return null;
            }

            double latitude = 0, longitude = 0;
            int width = 0, height = 0;
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(imageFile.FilePath);
                var location = GetGeoLocation(directories);
                if (location != null)
                {
                    latitude = location.Value.Latitude;
                    longitude = location.Value.Longitude;
                }
                else
                {
                    Log.Debug("No geolocation found in GPS data for file: {File}", imageFile.FilePath);
                }

                var dimensions = GetImageDimensions(directories);
                if (dimensions.Width != 0 && dimensions.Height != 0)
                {
                    width = dimensions.Width;
                    height = dimensions.Height;
                }
                else
                {
                    Log.Debug("No image dimensions found for file: {File}", imageFile.FilePath);
                }
            }
            catch
            {
                Log.Warning("Failed to read metadata for file: {File}", imageFile.FilePath);
            }

            return new ImageInfo(imageFile, width, height, latitude, longitude);
        }

        /// <summary>
        /// Gets the geographical location (latitude and longitude) from the specified metadata directories.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <returns>A GeoLocation object containing the latitude and longitude, or null if not found</returns>
        private static GeoLocation? GetGeoLocation(IEnumerable<MetadataExtractor.Directory> directories)
        {
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gps != null && gps.TryGetGeoLocation(out GeoLocation location))
            {
                return location;
            }
            return null;
        }

        /// <summary>
        /// Gets the image dimensions (width and height) from the specified metadata directories.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <returns>A Dimensions object containing the image's width and height</returns>
        private static Dimensions GetImageDimensions(IEnumerable<MetadataExtractor.Directory> directories)
        {
            var exifDir = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (exifDir?.TryGetInt32(ExifDirectoryBase.TagImageWidth, out int exifWidth) == true &&
                exifDir.TryGetInt32(ExifDirectoryBase.TagImageHeight, out int exifHeight) == true)
            {
                return new Dimensions { Width = exifWidth, Height = exifHeight };
            }

            // Fallback to common directories if Exif data is not available

            var jpegDir = directories.OfType<JpegDirectory>().FirstOrDefault();
            if (jpegDir?.TryGetInt32(JpegDirectory.TagImageWidth, out int jpegWidth) == true &&
                jpegDir.TryGetInt32(JpegDirectory.TagImageHeight, out int jpegHeight) == true)
            {
                return new Dimensions { Width = jpegWidth, Height = jpegHeight };
            }

            var heifDir = directories.OfType<HeicImagePropertiesDirectory>().FirstOrDefault();
            if (heifDir?.TryGetInt32(HeicImagePropertiesDirectory.TagImageWidth, out int heifWidth) == true &&
                heifDir.TryGetInt32(HeicImagePropertiesDirectory.TagImageHeight, out int heifHeight) == true)
            {
                return new Dimensions { Width = heifWidth, Height = heifHeight };
            }

            var pngDir = directories.OfType<PngDirectory>().FirstOrDefault();
            if (pngDir?.TryGetInt32(PngDirectory.TagImageWidth, out int pngWidth) == true &&
                pngDir.TryGetInt32(PngDirectory.TagImageHeight, out int pngHeight) == true)
            {
                return new Dimensions { Width = pngWidth, Height = pngHeight };
            }

            var bmpDir = directories.OfType<BmpHeaderDirectory>().FirstOrDefault();
            if (bmpDir?.TryGetInt32(BmpHeaderDirectory.TagImageWidth, out int bmpWidth) == true &&
                bmpDir.TryGetInt32(BmpHeaderDirectory.TagImageHeight, out int bmpHeight) == true)
            {
                return new Dimensions { Width = bmpWidth, Height = bmpHeight };
            }

            var gifDir = directories.OfType<GifHeaderDirectory>().FirstOrDefault();
            if (gifDir?.TryGetInt32(GifHeaderDirectory.TagImageWidth, out int gifWidth) == true &&
                gifDir.TryGetInt32(GifHeaderDirectory.TagImageHeight, out int gifHeight) == true)
            {
                return new Dimensions { Width = gifWidth, Height = gifHeight };
            }

            var webpDir = directories.OfType<WebPDirectory>().FirstOrDefault();
            if (webpDir?.TryGetInt32(WebPDirectory.TagImageWidth, out int webpWidth) == true &&
                webpDir.TryGetInt32(WebPDirectory.TagImageHeight, out int webpHeight) == true)
            {
                return new Dimensions { Width = webpWidth, Height = webpHeight };
            }

            return new Dimensions { Width = 0, Height = 0 };
        }
    }
}