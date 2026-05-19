# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a server hosted application that processes images from a configured server folder and maps them based on geotagged metadata within the images.

This application is built using .NET and leverages the Leaflet.js library for map rendering. Aspire is used for hosting and orchestrating the application components, while MetadataExtractor is used to extract geotagged metadata from the images.

This project has partly been an experiment in using AI tools such as GitHub Copilot as a coding assistant particularly for initial scaffolding and unit test creation.
With strict human review and modification to ensure quality and correctness with an exact idea of the design, using the tools to assist rather than replace human decision-making.

Also to use Aspire as a hosting and orchestration tool for a .NET application, to learn about its capabilities and features.

The concepts here are not unique to .NET and could be implemented in any language or framework.

## Dependencies

- .NET 10 (likely can be retargeted to work with .NET 8+)
- [Aspire](https://aspire.dev/)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [Leaflet.js](https://leafletjs.com/)
- [openstreetmap.org](https://www.openstreetmap.org/)

## Components

- ImageMapper.Api - Back end API that fetches and processes image data
- ImageMapper.Web - Front end .NET Blazor web app that produces the UI to render the data on a map
- ImageMapper.Models - .NET class library of shared models

### Aspire components
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

or for multuple image folders:
```json
{
  "ImageFolders": [
	"/path/to/your/images1",
	"/path/to/your/images2"
  ]
}
```

`ImageFolder` setting takes precedence over `ImageFolders` if both are present
