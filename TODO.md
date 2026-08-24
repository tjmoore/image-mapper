# TODO

## UI

- Show file info in photo view, including filename, GPS coordinates, camera model, settings, date taken, etc
- Show small round thumbnail of image on map instead of just a pin
- Responsive design enhancements where required across different screen sizes, including mobile devices
- Filtering, search etc
- Show files without geolocation as a list on a separate page for example

## Backend

- GetImageBytesAsync - return stream through to client ImageFetcher which converts to stream anyway, as there's no API layer now
- Indexing / caching of image metadata
	- Update map while cache updates instead of needing to refresh the page
	- Detect folder/file changes instead of recaching on schedule
- Support for folder patterns in image folder config and/or folder exclusion
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container though may remove need to deploy appsettings.json for each environment.
- Error handling and logging improvements as required
- Support for varied image sources other than file folders, e.g. cloud storage, photo management software, etc.

## Deployment

- Convert to a library package of Blazor components and backend services for ease of use in other projects, e.g. as a NuGet package. UI provided serves as an example.
- Optional app wrapping, e.g. Electron or .NET MAUI (BlazorWebView), as alternative to deploying to a server, and/or to allow running as an app on mobile devices.

## General

- Further unit tests, including Blazor components, checking UI rendered output, etc
- Possibility of adding missing geolocation data to images without any, via a map or by entering coordinates manually and/or by using a reverse geocoding service to find the nearest known location
