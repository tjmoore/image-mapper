# TODO

- Indexing / caching of image metadata
	- Back-end caching of file list and metadata to avoid reprocessing on each request
	- Need to detect changes
- Optionally show metadata in enlarged photo view
- Support for folder patterns in image folder config
- Support for folder exclusion
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- UI improvements, filtering, search etc
	- Could use back-end cache instead of loading all into the browser and then processing
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container
- Error handling and logging improvements as required
- Support for varied image sources not just a file folder