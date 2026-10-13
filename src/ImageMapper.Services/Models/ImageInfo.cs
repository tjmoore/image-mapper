using System.Text.Json.Serialization;

namespace ImageMapper.Services.Models;

/// <summary>
/// Represents basic information about a file, including its unique identifier and file name.
/// </summary>
/// <param name="Id">The unique identifier of the file</param>
/// <param name="FileName">The file name</param>
/// <param name="FilePath">The full file path</param>
/// <param name="LastModified">The last modified date and time of the file</param>
/// <param name="Created">The creation date and time of the file</param>
/// <param name="ContentType">The MIME type of the file</param>
/// <param name="Url">The URL to access the file if applicable</param>
public record BasicFileInfo(
    string Id,
    string FileName,
    string FilePath,
    DateTimeOffset LastModified,
    DateTimeOffset Created,
    string ContentType,
    string Url = "");

/// <summary>
/// Represents information about an image, including its unique identifier, file name, and geographical coordinates (latitude and longitude).
/// </summary>
/// <param name="BasicFileInfo">The basic file information of the image</param>
/// <param name="Width">The width of the image in pixels</param>
/// <param name="Height">The height of the image in pixels</param>
/// <param name="Latitude">The latitude coordinate of the image's location</param>
/// <param name="Longitude">The longitude coordinate of the image's location</param>
/// <param name="DateTimeOriginal">The original (usually local) date and time when the image was taken</param>
/// <param name="TimeZoneOffset">The time zone offset of the original date and time when the image was taken</param>
/// <param name="CameraMaker">The maker of the camera used to take the image</param>
/// <param name="CameraModel">The model of the camera used to take the image</param>
public record ImageInfo(
    BasicFileInfo BasicFileInfo,
    int Width,
    int Height,
    double Latitude,
    double Longitude,
    DateTimeOffset DateTimeOriginal,
    string TimeZoneOffset,
    string CameraMaker,
    string CameraModel) : BasicFileInfo(BasicFileInfo)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageInfo"/> record with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the image</param>
    /// <param name="fileName">The file name of the image</param>
    /// <param name="filePath">The full file path of the image</param>
    /// <param name="width">The width of the image in pixels</param>
    /// <param name="height">The height of the image in pixels</param>
    /// <param name="latitude">The latitude coordinate of the image's location</param>
    /// <param name="longitude">The longitude coordinate of the image's location</param>
    /// <param name="lastModified">The last modified date and time of the image</param>
    /// <param name="created">The creation date and time of the image</param>
    /// <param name="dateTimeOriginal">The original (usually local) date and time when the image was taken</param>
    /// <param name="timeZoneOffset">The time zone offset of the original date and time when the image was taken</param>
    /// <param name="contentType">The MIME type of the image</param>
    /// <param name="url">The URL to access the image if applicable</param>
    /// <param name="cameraMaker">The maker of the camera used to take the image</param>
    /// <param name="cameraModel">The model of the camera used to take the image</param>
    [JsonConstructor]
    public ImageInfo(string id, string fileName, string filePath, int width, int height, double latitude, double longitude, DateTimeOffset lastModified, DateTimeOffset created, DateTimeOffset dateTimeOriginal, string timeZoneOffset, string contentType, string url = "", string cameraMaker = "", string cameraModel = "")
        : this(new BasicFileInfo(id, fileName, filePath, lastModified, created, contentType, url), width, height, latitude, longitude, dateTimeOriginal, timeZoneOffset, cameraMaker, cameraModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageInfo"/> record with the specified <see cref="ImageInfo"/> and new URL.
    /// </summary>
    /// <param name="imageInfo">The existing <see cref="ImageInfo"/> instance</param>
    /// <param name="url">The new URL to associate with the image</param>
    public ImageInfo(ImageInfo imageInfo, string url)
        : this(new BasicFileInfo(imageInfo.Id, imageInfo.FileName, imageInfo.FilePath, imageInfo.LastModified, imageInfo.Created, imageInfo.ContentType, url), imageInfo.Width, imageInfo.Height, imageInfo.Latitude, imageInfo.Longitude, imageInfo.DateTimeOriginal, imageInfo.TimeZoneOffset, imageInfo.CameraMaker, imageInfo.CameraModel)
    {
    }
}
