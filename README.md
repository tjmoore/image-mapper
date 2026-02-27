# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a server hosted application that processes images from a configured server folder and maps them based on geotagged metadata within the images.

This application is built using .NET and leverages the Leaflet.js library for map rendering

Initial development has partly been an experiment in using AI tools to assist in code generation, architecture design, and unit test creation.
With strict human review and modification to ensure quality and correctness.

## Dependencies

- .NET 10 (likely will work with .NET 8+)
- [Aspire](https://aspire.dev/)
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

- Support multiple image folders
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- Caching. Memory and/or stored cache of processed image metadata to speed up subsequent loads and reduce processing on each request. Would need to detect changes however.
- UI improvements, filtering etc
- Error handling and logging improvements
- Container support
- Configurable map tile provider options?
- Support for varied image sources not just a file folder
