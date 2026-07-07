namespace ImageMapper.Models;

/// <summary>
/// Represents basic information about a file, including its unique identifier and file name.
/// </summary>
/// <param name="Id">The unique identifier of the file</param>
/// <param name="FileName">The file name</param>
/// <param name="FilePath">The full file path</param>
public record BasicFileInfo(
    string Id,
    string FileName,
    string FilePath);

/// <summary>
/// Represents information about an image, including its unique identifier, file name, and geographical coordinates (latitude and longitude).
/// </summary>
/// <param name="BasicFileInfo">The basic file information of the image</param>
/// <param name="Latitude">The latitude coordinate of the image's location</param>
/// <param name="Longitude">The longitude coordinate of the image's location</param>
public record ImageInfo(
    BasicFileInfo BasicFileInfo,
    double Latitude,
    double Longitude) : BasicFileInfo(BasicFileInfo);
