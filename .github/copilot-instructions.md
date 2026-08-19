# Copilot instructions

## Overview

ImageMapper is a .NET application that scans configured folders for images, extracts metadata (including geolocation), and renders them on a map using a Blazor frontend. It is built with [.NET Aspire](https://aspire.dev) to orchestrate its services and provide a seamless development experience.

Refer also to repository docs (README.md, AGENTS.md) for further context

## Coding style and guidelines
- Follow SOLID principles and clean code practices. Don't over-engineer; keep it simple and maintainable.
- Follow .NET conventions (PascalCase for types and methods, camelCase for local variables).
- Use async/await for asynchronous operations; avoid blocking calls.
- Use consistent naming conventions
- For Blazor components
	- Use CSS Isolation and do not use inline styles. Global CSS in app.css for shared styles.
	- Aim for responsive design. Avoid fixed widths and heights where possible; use relative units and CSS flex/grid layouts.
	- Prefer C# and Blazor event handling and data binding over JavaScript.
	- Use JS Interop from C# where necessary, and DotNet interop from JS where necessary, but keep JS minimal.
	- Avoid direct DOM manipulation in JS; use Blazor's rendering model instead.
	- Use TypeScript and .ts files. Do not modify generated .js files.

## Architecture

- **ImageMapper.Api** — Backend service that enumerates images, extracts EXIF/geolocation metadata, and exposes endpoints
- **ImageMapper.Web** — Blazor frontend that consumes the API and renders images on a map using Leaflet.js (OpenStreetMap)
- **ImageMapper.ServiceDefaults** — Aspire extensions for service discovery, health checks, and telemetry
- **ImageMapper.AppHost** — The Aspire host that composes and runs the services in development
- **ImageMapper.Models** — Shared models used by both API and Web projects
- **ImageMapper.Tests** — NUnit tests covering core services and API flows

- Runtime flow:
  1. AppHost (aspire) starts configured resources and services.
  2. Api scans configured ImageFolders, uses MetadataExtractor to pull EXIF/geotag info and builds ImageInfo objects.
  3. Web (Blazor + Leaflet) fetches image metadata from Api and renders markers on the map.
- Configuration:
  - ImageFolders (array) lives in appsettings.json for ImageMapper.Api.

## Build

- Restore & build solution (root):
  - dotnet restore ImageMapper.slnx && dotnet build ImageMapper.slnx -c Debug
- Run development environment (preferred):
  - aspire run
  - Alternative (run AppHost directly): dotnet run --project src\ImageMapper.AppHost\ImageMapper.AppHost.csproj
- Run individual projects:
  - API: dotnet run --project src\ImageMapper.Api\ImageMapper.Api.csproj
  - Web (Blazor): dotnet run --project src\ImageMapper.Web\ImageMapper.Web.csproj

## Testing

- Run all tests: dotnet test src\ImageMapper.Tests\ImageMapper.Tests.csproj
- Run a single test method (example using fully-qualified name):
dotnet test src\ImageMapper.Tests\ImageMapper.Tests.csproj --filter "FullyQualifiedName~ImageMapper.Tests.ImagesApiTest.GetImageBytesAsyncReturnsValidImageBytes"
- Alternative single-test filter (partial display name):
dotnet test --filter "DisplayName~PartialTestName"

## Key conventions and patterns

- Aspire-first workflow:
  - apphost.cs (ImageMapper.AppHost) defines resources. Changes to apphost.cs require restarting the AppHost (aspire run).
  - Use `aspire run` when iterating on multi-component behavior — it reliably composes Api + Web + diagnostics.
- Configuration keys:
  - "ImageFolders" (array of one or more paths).
- Tests & temporary assets:
  - NUnit is used and should follow arrange/act/assert pattern
  - Tests use TestContext.CurrentContext.TestDirectory where temporary files are needed
- Async enumeration and cancellation:
  - IAsyncEnumerable<T> producers in services honor cancellation tokens. Keep using the `[EnumeratorCancellation]` attribute on CancellationToken parameters where appropriate.
- Use swagger / OpenAPI for API documentation and testing. The Api project exposes Swagger UI at /swagger when running.

## Containerisation

- Do not directly generate Dockerfiles or docker-compose.yml, use apire publish to generate
- aspire apphost.cs should define the configuration for docker-compose.yml and .env generation
- generate docker-compose.yml and unfilled .env  when running `aspire publish`
- do as above with environment specific .env and build container images, when running `aspire do prepare-compose --environment <env>` for Staging or Prodution environment
- do as above and deploy with docker compose up for example, when running `aspire deploy` and optional `--environment <env>` for Staging or Production environment (default Production)
