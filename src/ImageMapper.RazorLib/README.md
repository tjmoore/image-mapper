# ImageMapper.RazorLib

Library of .NET Blazor components to render data generated from ImageMapper.Services on a map

## Dependencies

`ImageMapper.Services` is a required dependency for this library, providing the services to extract metadata from images and data to front end consumers.

## Installation

If you have the source code, you can add a reference to the `ImageMapper.RazorLib` project in your Blazor application.

Alternatively, you can install the NuGet packages from GitHub. You will need a GitHub Personal Access Token with `read:packages` scope to access the packages.

`<your-github-username>` and `<your-github-personal-access-token>` should be replaced with your GitHub username and personal access token respectively.

<version> is the version of the packages you want to install.

```bash
dotnet nuget add source https://nuget.pkg.github.com/tjmoore/index.json --name github-tjmoore --username <your-github-username> --password <your-github-personal-access-token> --store-password-in-clear-text
dotnet add package ImageMapper.Services --version <version>
dotnet add package ImageMapper.RazorLib --version <version>
```

The nuget add source command will add to nuget.config in your repo except the credentials which will be stored in your user profile.

However you may also need to add mapping to your nuget.config file in your repo to find the packages, for example:
```xml
  <packageSourceMapping>
    <packageSource key="https://api.nuget.org/v3/index.json">
      <package pattern="*" />
    </packageSource>
    <packageSource key="github-tjmoore">
      <package pattern="ImageMapper.*" />
    </packageSource>
  </packageSourceMapping>
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
...
app.MapStaticAssets();
```

Ensure `app.MapStaticAssets()` is called to serve static assets from the ImageMapper.RazorLib package, such as CSS and JS files. Typically before MapRazorComponents.

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


You will also need to add the required CSS and JS references for Leaflet libraries to display images on a map, to your `wwwroot/index.html`, `App.razor` file or similar, for example:
```html
<head>
    ...
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <link rel="stylesheet" href="https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.css" />
    <link rel="stylesheet" href="https://unpkg.com/leaflet.markercluster@1.5.3/dist/MarkerCluster.Default.css" />
    ...
</head>

<script>
    ...
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script src="https://unpkg.com/leaflet.markercluster@1.5.3/dist/leaflet.markercluster.js"></script>
    ...
</script>
```

