# TODO

## UI

- Show file further detail in info panel such as camera model, settings, date taken, etc
- Show small round thumbnail of image on map instead of just a pin
- Responsive design enhancements where required across different screen sizes, including mobile devices
- Filtering, search etc
- Show files without geolocation as a list on a separate page for example
- ImageSource.ts could be wrapped as a Razor component to use in place of img elements
	- MapSection however uses template for pop-up used by leaflet, and rendering is outside of Blazor
	- Leaflect may neeed to be wrapped as a Razor component as well, or use a Blazor Leaflet wrapper library

## Backend

- Indexing / caching of image metadata
	- Update map while cache updates instead of needing to refresh the page
	- Detect folder/file changes instead of recaching on schedule
- Support for folder patterns in image folder config and/or folder exclusion
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container.
- Error handling and logging improvements as required
- Support for varied image sources other than file folders, e.g. cloud storage, photo management software, etc.

## Deployment

- Publish NuGet package(s) on release for library projects.
- Optional app wrapping, e.g. Electron or .NET MAUI (BlazorWebView), as alternative to deploying to a server, and/or to allow running as an app on mobile devices.

## General

- Further unit tests, including Blazor components, checking UI rendered output, etc
- Possibility of adding missing geolocation data to images without any, via a map or by entering coordinates manually and/or by using a reverse geocoding service to find the nearest known location
