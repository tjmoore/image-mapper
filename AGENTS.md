# Project: ImageMapper

Refer to .github/copilot-instructions.md for agent-specific build and test commands.

## Overview

ImageMapper is a .NET application that scans configured folders for images, extracts metadata (including geolocation), and renders them on a map using a Blazor frontend. It is built with [.NET Aspire](https://aspire.dev) to orchestrate its services and provide a seamless development experience.

This guide covers key concepts and workflows for working with an Aspire-based project.

## Quick Start

To run the application and start development:

```bash
aspire run
```
	
This starts the Aspire dashboard and orchestrates all configured resources (API, Web, etc.). If an instance is already running, you'll be prompted to stop it first.

The dashboard provides a central view of service health, logs, and traces.

## Architecture & Resources

The solution is structured into several projects, each serving a specific role:

- **ImageMapper.Api** — Backend service that enumerates images, extracts EXIF/geolocation metadata, and exposes endpoints
- **ImageMapper.Web** — Blazor frontend that consumes the API and renders images on a map using Leaflet.js (OpenStreetMap)
- **ImageMapper.ServiceDefaults** — Aspire extensions for service discovery, health checks, and telemetry
- **ImageMapper.AppHost** — The Aspire host that composes and runs the services in development
- **ImageMapper.Models** — Shared models used by both API and Web projects
- **ImageMapper.Tests** — NUnit tests covering core services and API flows

Configuration is managed through `appsettings.json` files in each project. The API image folders are configured via the `ImageFolders` (array) key.

## Development Workflow

### Best Practices

1. **Run before changes** — Start with `aspire run` and inspect resource health to establish a known state
2. **Incremental changes** — Make small, focused edits and validate with `aspire run`
3. **Restart on apphost.cs changes** — Modifications to `apphost.cs` require stopping and restarting the Aspire host
4. **Test early** — Use the dashboard and browser to verify behavior after each change

### Making Changes

- **Code changes** (Services, Controllers, Components) — Reload with individual resource restarts or full app restart as needed
- **AppHost changes** (adding resources, modifying composition) — Full restart of `aspire run` required
- **Configuration changes** (appsettings.json) — Restart Aspire for changes to take effect

## Diagnostics & Debugging

Aspire captures detailed logs and distributed traces for all resources. Before making changes to debug an issue:

1. **Check resource health** — Use the Aspire dashboard to see resource status and any error indicators
2. **Review logs** — Access console output and structured logs from the dashboard for each resource
3. **Examine traces** — Use distributed trace data to understand request flows and timing
4. **Analyze trace logs** — View logs associated with specific traces for deeper diagnostics

The dashboard is the primary tool for monitoring and understanding what's happening in your services.

## Key Files

- [README.md](README.md) — Overview, supported image formats, running instructions
- [apphost.cs](src/ImageMapper.AppHost/AppHost.cs) — Aspire resource definitions and composition
- [appsettings.json](src/ImageMapper.Api/appsettings.json) — Configuration (image folders, etc.)
- [.github/copilot-instructions.md](.github/copilot-instructions.md) — Agent-specific build and test commands

## Related Resources

- **Aspire Official Docs** — https://aspire.dev and https://learn.microsoft.com/dotnet/aspire
- **NuGet Packages** — https://nuget.org (for integration version details and compatibility)
- **Playwright Testing** — This repo includes Playwright support for functional testing of resources

## Notes

- **Aspire workload** is obsolete; use the Aspire CLI (`aspire run`) instead
- **Persistent containers** should generally be avoided early in development to prevent state management issues when restarting
- When updating the apphost, use `aspire update` to refresh the SDK and core packages; you may need to manually update other dependencies for compatibility