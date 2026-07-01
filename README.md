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

The image folders are configured via `appsettings.<environment>.json` file in ImageMapper.Api project for the relevant environment built.
This is generally used in development and/or when not deploying a container.

```json
{
  "ImageFolders": [
	"/path/to/your/images1",
	"/path/to/your/images2"
  ]
}
```

## Container deployment (Aspire CLI)

`ImageMapper.AppHost` controls Docker Compose artifact generation for:

- `aspire publish`
- `aspire do prepare-compose --environment <Staging|Production>`
- `aspire deploy --environment <Staging|Production>`

Compose environment defaults are configured in `src/ImageMapper.AppHost/appsettings.<environment>.json` under `ComposeDefaults` instead of `appsettings.json` within the API project.
Additionally the ports the container images listen on can be configured here. These are the internal ports.

```json
{
  "ComposeDefaults": {
    "ImageFolders": [
        "/path/to/your/images1",
        "/path/to/your/images2"
    ],
    "ApiPort": 8081,
    "WebPort": 8080
  }
}
```

Replace the `CHANGE_ME_*` default values in the files with the folder paths for each deployment environment.

You will likely need to add a bind mount for the image folders to resolve them outside of the container, which you can do by creating a `docker-compose.override.yaml` file in `src\ImageMapper.AppHost\aspire-output`, for example:

```yaml
services:
  imagemapper-api:
    volumes:
      - "/path/to/your/images:/data/images:ro"
```

Make sure relevant permissions exist for the source folder. When using Docker Desktop on Windows, you may need to add the source folder to the list of shared drives in Docker Desktop settings.

### Issues

You also may need to start with the 'aspire do prepare-compose' command to generate the compose files, add the override file and then run
`docker compose --env-file .\.env.staging up -d --remove-orphans` from `src\ImageMapper.AppHost\aspire-output`.

Also may need to initially run `aspire deploy` to get all the images built correctly, then delete the container it creates and run the `docker compose up` command to create a container
picking up the override file. `aspire deploy` appears to fail to pick up the override, but `aspire do prepare-compose` fails to correctly tag images or create the dashboard image.

Issues with Aspire generating bind/volume mounts on project resources in the compose file from code, discussed here https://github.com/microsoft/aspire/issues/4359


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
