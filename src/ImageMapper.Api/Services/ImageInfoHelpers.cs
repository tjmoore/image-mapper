using ImageMapper.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;

namespace ImageMapper.Api.Services
{
    internal static class ImageInfoHelpers
    {
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
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(imageFile.FilePath);
                var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
                if (gps != null)
                {
                    if (gps.TryGetGeoLocation(out GeoLocation location))
                    {
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                    }
                    else
                    {
                        Log.Debug("No geolocation found in GPS data for file: {File}", imageFile.FilePath);
                    }
                }
            }
            catch
            {
                Log.Warning("Failed to read metadata for file: {File}", imageFile.FilePath);
            }

            return new ImageInfo(imageFile, latitude, longitude);
        }
    }
}