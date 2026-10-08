# ImageMapper.RazorLib

Library of .NET Blazor components to render data generated from ImageMapper.Services on a map

## Dependencies

`ImageMapper.Services` is a required dependency for this library, providing the services to extract metadata from images and data to front end consumers.

## Installation

If you have the source code, you can add a reference to the `ImageMapper.RazorLib` project in your Blazor application.

Alternatively, you can install the NuGet package from GitHub. You will need a GitHub personal access token with `read:packages` scope to access the package.

<repo-owner> is the name of the repository owner where the package is hosted, and `<your-github-username>` and `<your-github-personal-access-token>` should be replaced with your GitHub username and personal access token respectively.

<version> is the version of the package you want to install.

```bash
dotnet nuget add source https://nuget.pkg.github.com/<repo-owner>/index.json --name github --username <your-github-username> --password <your-github-personal-access-token>
dotnet add package ImageMapper.Services --version <version>
dotnet add package ImageMapper.RazorLib --version <version>
```

### Configuration

The image folders are configured via `appsettings.<environment>.json` file in client project for the relevant environment built.
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

The `ImageMapper.RazorLib` library provides a Razor component that can be used to display images on a map, by adding a reference to the project or NuGet package, and adding the services to the DI container in `Program.cs`:
```csharp
using ImageMapper.RazorLib;
using ImageMapper.Services;
...
var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddImageMapperRazorLib();
builder.Services.AddImageMapperServices();
```

`AddImageMapperRazorLib()` is an extension method that adds the required Razor components to the DI container

`AddImageMapperServices()` is an extension method that adds the required services to the DI container, including a worker service to fetch image information

To use the `ImageMap` Razor component, for example add the following to your Razor page:
```razor
@page "/"
@using ImageMapper.RazorLib.Components
@rendermode InteractiveServer

<PageTitle>Image Map</PageTitle>

<ImageMap />
```