# Copilot instructions for ImageMapper

Purpose
- Provide concise, repository-specific instructions for Copilot-style assistants and future contributors. Prefer repository docs first (README.md, AGENTS.md) for deeper context.

Build, test, and lint commands
- Restore & build solution (root):
  - dotnet restore ImageMapper.slnx && dotnet build ImageMapper.slnx -c Debug
- Run development environment (preferred):
  - aspire run
  - Alternative (run AppHost directly): dotnet run --project src\ImageMapper.AppHost\ImageMapper.AppHost.csproj
- Run individual projects:
  - API: dotnet run --project src\ImageMapper.Api\ImageMapper.Api.csproj
  - Web (Blazor): dotnet run --project src\ImageMapper.Web\ImageMapper.Web.csproj
- Tests:
  - Run all tests: dotnet test src\ImageMapper.Tests\ImageMapper.Tests.csproj
  - Run a single test method (example using fully-qualified name):
    dotnet test src\ImageMapper.Tests\ImageMapper.Tests.csproj --filter "FullyQualifiedName~ImageMapper.Tests.ImagesApiTest.GetImageBytesAsyncReturnsValidImageBytes"
  - Alternative single-test filter (partial display name):
    dotnet test --filter "DisplayName~PartialTestName"
- Formatting / linting:
  - No repository linter/formatter is configured by default. To run dotnet-format (optional):
    dotnet tool install --global dotnet-format
    dotnet format ImageMapper.slnx

High-level architecture (big picture)
- Solution layout:
  - ImageMapper.Api: backend API that enumerates images from configured folder(s), extracts metadata and exposes endpoints.
  - ImageMapper.Web: front-end Blazor app that consumes the API and renders maps using Leaflet.js (OpenStreetMap).
  - ImageMapper.Models: shared POCO models used by Api and Web.
  - ImageMapper.AppHost: Aspire AppHost that composes and runs the services for development (apphost.cs defines resources).
  - ImageMapper.ServiceDefaults: Aspire-related extensions (service discovery, health checks, telemetry).
  - ImageMapper.Tests: NUnit tests covering core services (ImageService, API flows).
- Runtime flow:
  1. AppHost (aspire) starts configured resources and services.
  2. Api scans ImageFolder(s), uses MetadataExtractor to pull EXIF/geotag info and builds ImageInfo objects.
  3. Web (Blazor + Leaflet) fetches image metadata from Api and renders markers on the map.
- Configuration:
  - ImageFolder / ImageFolders keys live in appsettings.json for ImageMapper.Api. ImageFolder takes precedence over ImageFolders.

Key conventions and patterns (repo-specific)
- Aspire-first workflow:
  - apphost.cs (ImageMapper.AppHost) defines resources. Changes to apphost.cs require restarting the AppHost (aspire run).
  - Use `aspire run` when iterating on multi-component behavior — it reliably composes Api + Web + diagnostics.
- Configuration keys:
  - "ImageFolder" (single path) and "ImageFolders" (array). The code prioritizes ImageFolder if both present.
- Tests & temporary assets:
  - NUnit is used (OneTimeSetUp / Test / TestCase). Tests create temporary image files using magic bytes and cleanup after run.
  - Tests use TestContext.CurrentContext.TestDirectory for temporary test folders.
- Async enumeration and cancellation:
  - IAsyncEnumerable<T> producers in services honor cancellation tokens. Keep using the `[EnumeratorCancellation]` attribute on CancellationToken parameters where appropriate.
- Project structure conventions:
  - Each project has its .csproj under src. Use explicit --project when running dotnet commands for a single project.

Important files to reference quickly
- README.md — usage, supported image formats, run instructions.
- AGENTS.md — Aspire and MCP tooling guidance (lists Playwright MCP server usage in this repo).

Notes for assistants (Copilot / AI)
- Prefer repository docs (README.md, AGENTS.md) for environment specifics (Aspire usage, Playwright MCP server configured).
- When suggesting edits that affect apphost.cs or service composition, recommend running `aspire run` and checking resource status via the Aspire MCP tools.
- Keep changes surgical; follow existing patterns in ServiceDefaults and AppHost.

MCP servers
- This repo already documents Playwright MCP server usage in AGENTS.md. Ask the developer before changing MCP server configuration.

References
- README.md
- AGENTS.md

--
(Generated: concise, repo-specific Copilot instructions.)