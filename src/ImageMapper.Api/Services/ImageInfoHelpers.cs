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
        /// <param name="filepath">The full path of the image file</param>
        /// <returns>An ImageInfo object containing the image's ID, filename, latitude, and longitude</returns>
        public static ImageInfo GetImageInfo(string filepath)
        {
            var id = ImageFetcherHelpers.GenerateIdForPath(filepath);
            var filename = Path.GetFileName(filepath);
            double latitude = 0, longitude = 0;
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filepath);
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
                        Log.Debug("No geolocation found in GPS data for file: {File}", filepath);
                    }
                }
            }
            catch
            {
                Log.Warning("Failed to read metadata for file: {File}", filepath);
            }

            return new ImageInfo(id, filename, latitude, longitude);
        }
    }
}