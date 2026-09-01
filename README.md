# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a .NET library and example application that scans images, extracts metadata including geolocation, and renders them on a map.

It is built with Blazor for the front-end components with back-end services to extract metadata from images, and uses [.NET Aspire](https://aspire.dev) to orchestrate.

## Dependencies

- .NET 10 (likely can be retargeted to work with .NET 8+)
- [Aspire](https://aspire.dev/)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [Leaflet.js](https://leafletjs.com/)
- [openstreetmap.org](https://www.openstreetmap.org/)

## Components

- ImageMapper.Services - Library of .NET services to extract metadata from images and provide data front end consumers
- ImageMapper.Models - .NET class library of shared models
- ImageMapper.Web - Example front end .NET Blazor web app that produces the UI to render the data on a map

### Aspire components
- ImageMapper.AppHost - .NET Aspire orchestrator to run and debug in a development environment
- ImageMapper.ServiceDefaults - Extensions for .NET Aspire support including service discovery, health checks and telemetry

## Running sample development environment

This runs the .NET Aspire host, launching the components and dashboard in the browser showing the service status allowing browsing to the example web UI

#### Visual Studio
Set `ImageMapper.AppHost` as start up project and run (F5)

#### Visual Studio Code

- Install Aspire CLI - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

- Install Aspire Extension - https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/aspire-vscode-extension

- Run with F5 or Run -> Start Debugging

Alternatively without the Aspire CLI / Extension, from Solution Explorer right click `ImageMapper.AppHost` and select Debug -> Start New Instance

#### Command Line
- Install Aspire CLI - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

- Run `aspire run`

Alternatively without Aspire CLI, run `dotnet run --project ImageMapper.AppHost`

This will run the .NET Aspire host, launching the components and dashboard in the browser showing the service status.

Launch the front end application from imagemapper-web link


### Configuration

The image folders are configured via `appsettings.<environment>.json` file in ImageMapper.Web project for the relevant environment built.
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

### Service Library usage

A client application can use the ImageMapper.Services library by adding a reference to the project or NuGet package, and adding the services to the DI container in `Program.cs`:

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

To get a stream of the image bytes for a specific image, call `GetImageBytesAsync(string filePath)` from an instance of `IImageService` using the ID of the image:

```csharp
public async Task FetchImageBytesAsync(IImageService imageService, string id)
{
	byte[]? imageBytes = await imageService.GetImageBytesAsync(id);
	// Process the image bytes as needed
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


`ImageMapper.Web` is an example front end application that uses the services to display the images on a map, and can be used as a reference for how to use the services in your own application.

TODO: The Blazor components may be also extracted into a library package for use in other web applications and the example will then show their use.


## Supported Image Formats

Based on support in MetadataExtractor

### Standard Image Formats

- jpg / jpeg — JPEG Image
- png — Portable Network Graphics
- gif — Graphics Interchange Format
- bmp — Bitmap Image
- heic — High Efficiency Image Container
- heif — High Efficiency Image Format
- ico — Windows Icon File
- webp — WebP Image
- pcx — PC Paintbrush Image
- tif / tiff — Tagged Image File Format

### RAW Camera Formats

- nef — Nikon Electronic Format (RAW)
- crw — Canon RAW (CRW)
- cr2 — Canon RAW (CR2)
- orf — Olympus RAW Image
- arw — Sony RAW Image
- raf — Fujifilm RAW Image
- srw — Samsung RAW Image
- x3f — Sigma RAW Image
- rw2 — Panasonic RAW Image
- rwl — Leica RAW Image
- dcr — Kodak RAW Image
- dng — Digital Negative (Adobe)


## Notes

This project has partly been an learning exercise in using tools such as GitHub Copilot as a coding assistant. This is a human-designed application where AI tools have been used at times to assist in the coding process, and other times purely human development. Reviews are human or where automated only with human final approval.

It has also been an exercise in using Aspire as a hosting and orchestration tool for a .NET application, to learn about its capabilities and features.
Aspire isn't necessary to use ImageMapper, it just aids in development and deployment.

This isn't unique in that there are other applications that do similar things, but this is a simple example of how to implement it in .NET with a Blazor front end and .NET back end services.
