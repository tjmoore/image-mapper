# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a server hosted application that processes images from a configured server folder,
extracts GPS metadata, and generates an interactive map displaying the locations of these images

This application is built using .NET and leverages the Leaflet.js library for map rendering

## Dependencies

- .NET 9
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
From Solution Explorer, right click `ImageMapper.AppHost` and select Debug -> Start New Instance

#### Command Line
Run `dotnet run --project ImageMapper.AppHost`

This will run the .NET Aspire host, launching the components and dashboard in the browser showing the service status.

Launch the front end application from imagemapper-web link

Alternatively using Aspire CLI:

`aspire run`

## Configuration

The folder for images can be configured via `appsettings.json` file in ImageMapper.Api project
```json
{
  "ImageFolder": "C:\\Path\\To\\Your\\Images"
}
```

## TODO

- Unit tests
- Optimise image processing for large number of images and folders
- Caching (minimally in-memory/redis etc, future database caching of metadata. With detection of image file changes)
- Configure map tile provider options
- UI improvements, filtering etc
- Optimise image loading via JS interop instead of API call
- or Blazor component library for map display? - BlazorLeaflet or similar wrapper package
- Error handling and logging improvements
- Container support
