# ImageMapper

[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/dotnet-10-blue.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![Blazor](https://img.shields.io/badge/blazor-UI-blue.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![TypeScript](https://img.shields.io/badge/typescript-UI-blue.svg)](https://www.typescriptlang.org/)
[![Aspire](https://img.shields.io/badge/aspire-DevOps-blue.svg)](https://aspire.dev/)\
[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)
[![Issues](https://img.shields.io/github/issues/tjmoore/image-mapper)](https://github.com/tjmoore/image-mapper/issues)
[![Release](https://img.shields.io/github/v/release/tjmoore/image-mapper)](https://github.com/tjmoore/image-mapper/releases)


ImageMapper is a .NET library and example application that processes a collection of images, extracts metadata including geolocation, and renders them on a map.

It is built with Blazor for the front-end components with back-end services to extract metadata from images.

## Requirements

- .NET 10 SDK (or later)
- [Aspire](https://aspire.dev/) (optional. Used in AppHost to aid development orchestration and debugging)

## Dependencies

Key dependencies used in this project include:

- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [Leaflet.js](https://leafletjs.com/)
- [openstreetmap.org](https://www.openstreetmap.org/)

## Components

- ImageMapper.Services - Library of .NET services to extract metadata from images and provide data to front end consumers
- ImageMapper.RazorLib - Library of .NET Blazor components to render the data on a map
- ImageMapper.Models - .NET class library of shared models
- ImageMapper.Web - Example front end .NET Blazor web app that produces the UI to render the data on a map

### Aspire components

- ImageMapper.AppHost - .NET Aspire orchestrator to run and debug in a development environment
- ImageMapper.ServiceDefaults - Extensions for .NET Aspire support including service discovery, health checks and telemetry

## Running sample development environment

This runs the .NET Aspire host, launching the components and dashboard in the browser showing the service status allowing browsing to the example web UI

#### Visual Studio

Set `ImageMapper.AppHost` as start up project and run (**F5**)

The web application can also be run independently of the Aspire host, by setting `ImageMapper.Web` as the start up project and running (**F5**)

#### Visual Studio Code

- Install **Aspire CLI** - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

- Install **Aspire Extension** - https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/aspire-vscode-extension

- Run with **F5** or **Run** -> **Start Debugging**

Alternatively without the Aspire CLI / Extension, from Explorer right click `ImageMapper.AppHost.csproj` and select Debug -> Start New Instance

The web application can also be run independently of the Aspire host, from Explorer right click `ImageMapper.Web.csproj` and select Debug -> Start New Instance

#### Command Line

- Install **Aspire CLI** - https://learn.microsoft.com/en-us/dotnet/aspire/cli/install

- Run `aspire run`

Alternatively without Aspire CLI, run `dotnet run --project ImageMapper.AppHost`

This will run the .NET Aspire host, launching the components and dashboard in the browser showing the service status.

Launch the front end application from imagemapper-web link

The web application can also be run independently of the Aspire host, by running `dotnet run --project ImageMapper.Web` from the command line.

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

- `ImageMapper.RazorLib` - See [ImageMapper.RazorLib/README.md](src/ImageMapper.RazorLib/README.md) for usage of the Razor components
- `ImageMapper.Services` - See [ImageMapper.Services/README.md](src/ImageMapper.Services/README.md) for usage of the services
- `ImageMapper.Web` is an example front end application that uses the services to display the images on a map, and can be used as a reference for how to use the services in your own application.


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

This project has been developed as a learning exercise in technologies used, and in the use of Aspire as an orchestration tool.

GitHub Copilot has been used strictly as a coding assistant in the sense of a pair programmer.
Much of the code is written by hand and all other suggested or generated code is carefully reviewed and understood.
Code reviews are human driven, or where automated with final approval by a human.
