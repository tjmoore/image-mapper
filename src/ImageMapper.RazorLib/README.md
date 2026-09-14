# ImageMapper.RazorLib

Library of .NET Blazor components to render data generated from ImageMapper.Services on a map

## Dependencies

`ImageMapper.Services` is a required dependency for this library, providing the services to extract metadata from images and data to front end consumers.

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

The `ImageMapper.RazorLib` library provides a Razor component that can be used to display images on a map, by adding a reference to the project or NuGet package, and adding the services to the DI container in `Program.cs`:
```csharp
using ImageMapper.RazorLib;
using ImageMapper.Services;
...
var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddImageMapperRazorLib();
builder.Services.AddImageMapperServices();
```

`AddImageMapperRazorLib()` is an extension method that adds the required Razor components to the DI container

`AddImageMapperServices()` is an extension method that adds the required services to the DI container, including a worker service to fetch image information

To use the `ImageMap` Razor component, for example add the following to your Razor page:
```razor
@page "/"
@using ImageMapper.RazorLib.Components
@rendermode InteractiveServer

<PageTitle>Image Map</PageTitle>

<ImageMap />
```