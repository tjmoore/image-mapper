using ImageMapper.Services.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Heif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using Serilog;

namespace ImageMapper.Services.Utils
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
            DateTimeOffset dateTimeOriginal = DateTimeOffset.MinValue;
            string timeZoneOffset = string.Empty;
            string cameraMaker = string.Empty;
            string cameraModel = string.Empty;
            string aperture = string.Empty, shutterSpeed = string.Empty, focalLength = string.Empty;
            int? iso = null;
            bool? flashFired = null;
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

                // Get the original date/time and offset of when the photo was taken from the metadata, or fallback to the file's creation date if not available
                // File creation date may not be accurate if the file was copied or moved, but it's the best we can do if the metadata is missing
                dateTimeOriginal = GetDateTimeOriginal(directories) ?? imageFile.Created;
                timeZoneOffset = GetTimeZoneOffset(directories, dateTimeOriginal);

                // Camera maker and model if available
                cameraMaker = GetIfd0Description(directories, ExifDirectoryBase.TagMake);
                cameraModel = GetIfd0Description(directories, ExifDirectoryBase.TagModel);

                // Camera settings if available
                aperture = GetSubIfdDescription(directories, ExifDirectoryBase.TagFNumber);
                shutterSpeed = GetSubIfdDescription(directories, ExifDirectoryBase.TagExposureTime);
                focalLength = GetSubIfdDescription(directories, ExifDirectoryBase.TagFocalLength);
                iso = GetSubIfdInt32(directories, ExifDirectoryBase.TagIsoEquivalent);
                flashFired = GetFlashFired(directories);
            }
            catch
            {
                Log.Warning("Failed to read metadata for file: {File}", imageFile.FilePath);
            }

            return new ImageInfo(imageFile, width, height, latitude, longitude, dateTimeOriginal, timeZoneOffset, cameraMaker, cameraModel,
                aperture, shutterSpeed, iso, focalLength, flashFired);
        }

        /// <summary>
        /// Gets the description of a tag from the ExifSubIfdDirectory in the specified metadata directories, or returns an empty string if not available.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <param name="tag">The tag to get the description for</param>
        /// <returns>The description of the tag, or an empty string if not available</returns>
        private static string GetSubIfdDescription(IEnumerable<MetadataExtractor.Directory> directories, int tag)
        {
            return directories.OfType<ExifSubIfdDirectory>().FirstOrDefault()?.GetDescription(tag) ?? string.Empty;
        }

        /// <summary>
        /// Gets the description of a tag from the ExifIfd0Directory in the specified metadata directories, or returns an empty string if not available.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <param name="tag">The tag to get the description for</param>
        /// <returns>The description of the tag, or an empty string if not available</returns>
        private static string GetIfd0Description(IEnumerable<MetadataExtractor.Directory> directories, int tag)
        {
            return directories.OfType<ExifIfd0Directory>().FirstOrDefault()?.GetDescription(tag) ?? string.Empty;
        }

        /// <summary>
        /// Gets the integer value of a tag from the ExifSubIfdDirectory in the specified metadata directories, or returns null if not available.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <param name="tag">The tag to get the integer value for</param>
        /// <returns>The integer value of the tag, or null if not available</returns>
        private static int? GetSubIfdInt32(IEnumerable<MetadataExtractor.Directory> directories, int tag)
        {
            var dir = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            return dir != null && dir.TryGetInt32(tag, out int value) ? value : null;
        }

        /// <summary>
        /// Gets whether the flash fired from the specified metadata directories, or returns null if not available.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <returns>True if the flash fired, false if it did not, or null if not available</returns>
        private static bool? GetFlashFired(IEnumerable<MetadataExtractor.Directory> directories)
        {
            return GetSubIfdInt32(directories, ExifDirectoryBase.TagFlash) is int flash ? (flash & 1) == 1 : null;
        }

        /// <summary>
        /// Gets the time zone offset from the specified metadata directories, or returns the local time zone offset of the dateTimeOriginal or created date if not available in the metadata.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <param name="dateTimeOriginal">The original date and time when the image was taken</param>
        /// <returns>The time zone offset as a string in the format "hh:mm"</returns>
        private static string GetTimeZoneOffset(IEnumerable<MetadataExtractor.Directory> directories, DateTimeOffset dateTimeOriginal)
        {
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSubIfdDirectory != null)
            {
                // EXIF OffsetTimeOriginal 0x9011 (Offset time of DateTimeOriginal), OffsetTimeDigitized 0x9012 (Offset time of DateTimeDigitized/CreateDate)
                // or OffsetTime 0x9010 (Offset time of ModifyDate within the file - not the file system's last modified date)
                string? timeZoneOffset = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagTimeZoneOriginal) ??
                    exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagTimeZoneDigitized) ??
                    exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagTimeZone);
                if (!string.IsNullOrEmpty(timeZoneOffset))
                {
                    return timeZoneOffset;
                }

                // Fallback to using the TimeZoneOffset tag if available, EXIF 0x882A (Time zone offset from UTC in hours in the first value of the array)
                var timeZoneOffsetArray = exifSubIfdDirectory.GetRationalArray(ExifDirectoryBase.TagTimeZoneOffset);
                if (timeZoneOffsetArray != null && timeZoneOffsetArray.Length > 0)
                {
                    // Convert the first offset value to a string representation
                    double offsetHours = timeZoneOffsetArray[0].ToDouble();
                    TimeSpan offset = TimeSpan.FromHours(offsetHours);
                    return offset.ToString(@"hh\:mm");
                }

                // If no timezone information is available, return the local timezone offset of the dateTimeOriginal object
                TimeSpan localOffset = dateTimeOriginal.Offset;
                // Format the offset as a string in the format "[+|-]hh:mm"
                return (localOffset < TimeSpan.Zero ? "-" : "+") + localOffset.ToString(@"hh\:mm");
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the original date and time when the image was taken if available from the specified metadata directories
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <returns>The original date and time when the image was taken, or the file's last modified date in EXIF data if not available, or null if neither is available</returns>
        private static DateTimeOffset? GetDateTimeOriginal(IEnumerable<MetadataExtractor.Directory> directories)
        {
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            // EXIF DateTimeOriginal 0x9003
            if (exifSubIfdDirectory != null && exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTimeOriginal))
            {
                return new DateTimeOffset(dateTimeOriginal);
            }

            // Fallback EXIF DateTimeDigitized / CreateDate 0x9004
            if (exifSubIfdDirectory != null && exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out DateTime dateTimeDigitized))
            {
                return new DateTimeOffset(dateTimeDigitized);
            }

            // Fallback to file's last modified time if DateTimeOriginal is not available
            var fileDir = directories.OfType<FileMetadataDirectory>().FirstOrDefault();
            if (fileDir != null && fileDir.TryGetDateTime(FileMetadataDirectory.TagFileModifiedDate, out DateTime fileModifiedDate))
            {
                return new DateTimeOffset(fileModifiedDate);
            }

            return null;
        }

        /// <summary>
        /// Gets the geographical location (latitude and longitude) from the specified metadata directories.
        /// </summary>
        /// <param name="directories">The metadata directories</param>
        /// <returns>A GeoLocation object containing the latitude and longitude, or null if not found</returns>
        private static GeoLocation? GetGeoLocation(IEnumerable<MetadataExtractor.Directory> directories)
        {
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
            return gps != null && gps.TryGetGeoLocation(out GeoLocation location) ? location : null;
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