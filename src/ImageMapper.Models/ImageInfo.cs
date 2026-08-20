using System.Text.Json.Serialization;

namespace ImageMapper.Models;

/// <summary>
/// Represents basic information about a file, including its unique identifier and file name.
/// </summary>
/// <param name="Id">The unique identifier of the file</param>
/// <param name="FileName">The file name</param>
/// <param name="FilePath">The full file path</param>
/// <param name="Url">The URL to access the file if applicable</param>
public record BasicFileInfo(
    string Id,
    string FileName,
    string FilePath,
    string Url = "");

/// <summary>
/// Represents information about an image, including its unique identifier, file name, and geographical coordinates (latitude and longitude).
/// </summary>
/// <param name="BasicFileInfo">The basic file information of the image</param>
/// <param name="Width">The width of the image in pixels</param>
/// <param name="Height">The height of the image in pixels</param>
/// <param name="Latitude">The latitude coordinate of the image's location</param>
/// <param name="Longitude">The longitude coordinate of the image's location</param>
public record ImageInfo(
    BasicFileInfo BasicFileInfo,
    int Width,
    int Height,
    double Latitude,
    double Longitude) : BasicFileInfo(BasicFileInfo)
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
    /// <param name="url">The URL to access the image if applicable</param>
    [JsonConstructor]
    public ImageInfo(string id, string fileName, string filePath, int width, int height, double latitude, double longitude, string url = "")
        : this(new BasicFileInfo(id, fileName, filePath, url), width, height, latitude, longitude)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageInfo"/> record with the specified <see cref="ImageInfo"/> and new URL.
    /// </summary>
    /// <param name="imageInfo">The existing <see cref="ImageInfo"/> instance</param>
    /// <param name="url">The new URL to associate with the image</param>
    public ImageInfo(ImageInfo imageInfo, string url)
        : this(new BasicFileInfo(imageInfo.Id, imageInfo.FileName, imageInfo.FilePath, url), imageInfo.Width, imageInfo.Height, imageInfo.Latitude, imageInfo.Longitude)
    {
    }
}
