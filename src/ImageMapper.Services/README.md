# ImageMapper.Services

Library of .NET services to extract metadata from images and provide data front end consumers

## Installation


### Configuration

The image folders are configured via `appsettings.<environment>.json` file in client project for the relevant environment built.
This is generally used in development and/or when not deploying a container.

```json
{
  "ImageFolders": [
	"/path/to/your/images1",
	"/path/to/your/images2"
  ]
}
```

## Usage

`ImageMapper.RazorLib` provides Razor components to render image on a map as processed by the services here. See [ImageMapper.RazorLib/README.md](src/ImageMapper.RazorLib/README.md).

Alternatively, a client application can also use this library directly:

```csharp
using ImageMapper.Services;
...
var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddImageMapperServices();
```

`AddImageMapperServices()` is an extension method that adds the required services to the DI container, including a worker service


To fetch a list of images as they are processed, call `GetImagesAsync()` from an instance of `IImageService` to retrieve the list of images with metadata.
This is an async enumerable, so you can iterate over the results as they are processed.

For example:

```csharp
public async Task FetchAndProcessImagesAsync(IImageService imageService)
{
	await foreach (ImageInfo? image in imageService.GetImagesAsync())
	{
		// Process each image as it is retrieved
		Console.WriteLine($"Image: ID: {image.Id}, FileName: {image.FileName}, Lon: {image.Longitude}, Lat: {image.Latitude}");
	}
}
```

To get a stream of the image for a specific image, call `GetImageStream(string id)` from an instance of `IImageService` using the ID of the image:

```csharp
public async Task FetchImageStreamAsync(IImageService imageService, string id)
{
	using Stream? imageStream = imageService.GetImageStream(id);
	if (imageStream != null)
	{
		// Process stream as needed, for example read into a byte array

		using var memoryStream = new MemoryStream();
		await imageStream.CopyToAsync(memoryStream);
		byte[] imageBytes = memoryStream.ToArray();
		// Process the image bytes as needed
	}
}
```

You can get a count of available images with `GetImageCount()`:
```csharp
public int GetImageCount(IImageService imageService) => imageService.GetImageCount();
```

As images are processed in the background and cached, the cache status can be checked through `CacheActivityStatus`:
```csharp
public void GetCacheStatus(ICacheActivityStatus cacheActivityStatus)
{
	CacheActivityStatus status = cacheActivityStatus.GetStatus();
	Console.WriteLine($"Cache Status: Is caching: {status.IsCaching}, Processed: {status.ProcessedCount}, Total: {status.TotalCount}");
}
```

A live stream of the status is also available through `CacheActivityStatus.GetStatusStream()` which returns an `IAsyncEnumerable<CacheActivityStatus>` that can be iterated over to receive updates as they occur:
```csharp
public async Task MonitorCacheStatusAsync(ICacheActivityStatus cacheActivityStatus)
{
	await foreach (CacheActivityStatus status in cacheActivityStatus.GetStatusStream())
	{
		Console.WriteLine($"Cache Status: Is caching: {status.IsCaching}, Processed: {status.ProcessedCount}, Total: {status.TotalCount}");
	}
}
```


`IImageService` and `ICacheActivityStatus` are registered in the DI container when calling `AddImageMapperServices()`, so they can be injected into your classes as needed.