# TODO

- Indexing / caching of image metadata
	- Back-end caching of file list and metadata to avoid reprocessing on each request
	- UI update as cache builds
	- Detect folder/file changes instead of recaching on schedule
- Show file info in photo view, including filename and GPS coordinates
- Support for folder patterns in image folder config and/or folder exclusion
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- UI improvements, filtering, search etc
	- Could use back-end cache instead of loading all into the browser and then processing
- Show files without geolocation as a list on a separate page for example
- Possibility of adding missing geolocation data to images without it, via a map or by entering coordinates manually and/or by using a reverse geocoding service to find the nearest known location
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container
- Error handling and logging improvements as required
- Support for varied image sources other than file folders