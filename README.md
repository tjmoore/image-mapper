# ImageMapper

[![Build](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml/badge.svg)](https://github.com/tjmoore/image-mapper/actions/workflows/build.yml)

ImageMapper is a .NET application that scans configured folders for images, extracts metadata (including geolocation), and renders them on a map using a Blazor frontend. It is built with [.NET Aspire](https://aspire.dev) to orchestrate its services and provide a seamless development experience.

This project has partly been an learning exercise in using tools such as GitHub Copilot. This is a human-designed application where AI tools have been used at times to assist in the coding process, and other times purely human development. Reviews are human or where automated only with human final approval.

It has also been an exercise in using Aspire as a hosting and orchestration tool for a .NET application, to learn about its capabilities and features.

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

The image folders are configured via `appsettings.json` file in ImageMapper.Api project. This is generally used in development and/or when not deploying a container.

```json
{
  "ImageFolders": [
	"/path/to/your/images1",
	"/path/to/your/images2"
  ]
}
```

## Container deployment configuration (Aspire CLI)

`ImageMapper.AppHost` controls Docker Compose artifact generation for:

- `aspire publish`
- `aspire do prepare-compose --environment <Staging|Production>`
- `aspire deploy --environment <Staging|Production>`

As an alternative to configuration in ImageMapper.Api `appsettings.json`, image folders can be configured with compose defaults for the environment.

Compose environment defaults are configured in `src/ImageMapper.AppHost/appsettings.<environment>.json` under `ComposeDefaults`:

```json
{
  "ComposeDefaults": {
    "ImageFolders": [
        "/path/to/your/images1",
        "/path/to/your/images2"
    ]
  }
}
```

Replace the `CHANGE_ME_*` default values in the files with the folder paths for each deployment environment.

Generated `.env` keys include optional override variables:

- `IMAGEMAPPER_API_IMAGE_FOLDERS_0`, `IMAGEMAPPER_API_IMAGE_FOLDERS_1`, ...

These keys are mapped to API configuration (`ImageFolders__<index>`) during compose publish/deploy flows.
`docker-compose.yaml` references these variables directly.
Environment-specific defaults are written into generated environment files during environment-targeted flows (for example `.env.Staging` for `aspire do prepare-compose --environment Staging`).
`aspire publish` keeps `.env` unfilled.

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
