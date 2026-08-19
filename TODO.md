# TODO

- Indexing / caching of image metadata
	- Update map while cache updates instead of needing to refresh the page
	- Detect folder/file changes instead of recaching on schedule
- Show file info in photo view, including filename and GPS coordinates
- Responsive design
- Further unit tests, including for API and Blazor components, checking UI rendered output
- UI improvements, filtering, search etc
	- Could use back-end cache instead of loading all into the browser and then processing
- Show files without geolocation as a list on a separate page for example
- Possibility of adding missing geolocation data to images without any, via a map or by entering coordinates manually and/or by using a reverse geocoding service to find the nearest known location
- Support for folder patterns in image folder config and/or folder exclusion
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container though may remove need to deploy appsettings.json for each environment.
- Error handling and logging improvements as required
- Support for varied image sources other than file folders
- Optional app wrapping, e.g. Electron or .NET MAUI (BlazorWebView), as alternative to deploying to a server, and/or to allow running on mobile devices.