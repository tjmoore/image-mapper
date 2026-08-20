# TODO

## UI

- Show file info in photo view, including filename, GPS coordinates, camera model, settings, date taken, etc
- Responsive design enhancements where required across different screen sizes, including mobile devices
- Filtering, search etc
- Show files without geolocation as a list on a separate page for example

## Backend

- Indexing / caching of image metadata
	- Update map while cache updates instead of needing to refresh the page
	- Detect folder/file changes instead of recaching on schedule
- Support for folder patterns in image folder config and/or folder exclusion
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container though may remove need to deploy appsettings.json for each environment.
- Error handling and logging improvements as required
- Support for varied image sources other than file folders, e.g. cloud storage, photo management software, etc.

## Deployment

- Simplified as much as possible, including for non-technical users, e.g. via a single command or script to deploy to a server or cloud service / container, and/or to run locally on a desktop or laptop computer
- Optional app wrapping, e.g. Electron or .NET MAUI (BlazorWebView), as alternative to deploying to a server, and/or to allow running as an app on mobile devices.
- As a plugin for existing photo management software if possible

## General

- Further unit tests, including for API and Blazor components, checking UI rendered output
- Possibility of adding missing geolocation data to images without any, via a map or by entering coordinates manually and/or by using a reverse geocoding service to find the nearest known location
