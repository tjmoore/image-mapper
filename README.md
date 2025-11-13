# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a server hosted application that processes images from a configured server folder,
extracts GPS metadata, and in a web application generates an interactive map displaying the locations of these images

This application is built using .NET and leverages the Leaflet.js library for map rendering

## Dependencies

- .NET 10 (likely will work with .NET 8+)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [Leaflet.js](https://leafletjs.com/)
- [openstreetmap.org](https://www.openstreetmap.org/)

## Components

- ImageMapper.Api - Back end API that fetches and processes image data
- ImageMapper.Web - Front end .NET Blazor web app that produces the UI to render the data on a map
- ImageMapper.Models - .NET class library of shared models
- ImageMapper.AppHost - .NET Aspire orchestrator to run and debug in a development environment
- ImageMapper.ServiceDefaults - Extensions for .NET Aspire support including service discovery, health checks and telemetry

## Running development environment

#### Visual Studio
Set `ImageMapper.AppHost` as start up project and run (F5)

#### Visual Studio Code

Install Aspire CLI - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

Install Aspire Extension - https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/aspire-vscode-extension

Run with F5 or Run -> Start Debugging

Alternatively without the Aspire CLI / Extension, from Solution Explorer right click `ImageMapper.AppHost` and select Debug -> Start New Instance

#### Command Line
Install Aspire CLI - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

Run `aspire run`

Alternatively without Aspire CLI, run `dotnet run --project ImageMapper.AppHost`

This will run the .NET Aspire host, launching the components and dashboard in the browser showing the service status.

Launch the front end application from imagemapper-web link


## Configuration

The folder for images can be configured via `appsettings.json` file in ImageMapper.Api project
```json
{
  "ImageFolder": "/path/to/your/images"
}
```

## TODO

- Further unit tests
- Optimise image processing for large number of images and folders
- Abstract file enumeration and loading to allow varied sources not just a file folder
- Caching (minimally in-memory/redis etc, future database caching of metadata. With detection of image file changes)
- Configure map tile provider options
- UI improvements, filtering etc
- Optimise Leaflet rendering from back-end images especially to handle large numbers of images
- Error handling and logging improvements
- Container support
