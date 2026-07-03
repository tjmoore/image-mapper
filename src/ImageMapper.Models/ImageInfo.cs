namespace ImageMapper.Models;

/// <summary>
/// Represents information about an image, including its unique identifier, file name, and geographical coordinates (latitude and longitude).
/// </summary>
/// <param name="Id">The unique identifier of the image</param>
/// <param name="FileName">The file name of the image</param>
/// <param name="Latitude">The latitude coordinate of the image's location</param>
/// <param name="Longitude">The longitude coordinate of the image's location</param>
public record ImageInfo(
    string Id,
    string FileName,
    double Latitude,
    double Longitude);

/// <summary>
/// Represents state of the image cache
/// </summary>
/// <param name="TotalImageFiles"></param>
/// <param name="Keys"></param>
public record ImageCacheInfo(
    int TotalImageFiles,
    int ProcesssedImageFiles,
    HashSet<string> Keys);