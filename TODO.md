# TODO

- Container deploy support
- Support for folder patterns in image folder config
- Support for folder exclusion
- Performance
	- Fetch from backend in batches for performance? or is IAsyncEnumerable yielding one file at a time sufficient? (i.e. HTTP chunked streaming)
	- Any benefit in websocket / HTTP/2 in backend for streaming metadata to frontend? chunked stream is still one open request
		- Updates from backend when new images are added to the folder, or metadata changes
	- Is separate backend API much use or adding a performance hit? Simplify as a library?
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- Indexing / caching of image metadata
	- To speed up subsequent loads and reduce processing on each request
	- Allows for adding search features and maybe AI support through MCP perhaps
	- Need to detect changes
- UI improvements, filtering, search etc
- Configurability in UI for folder selection, map tile provider, etc. Needs persistence when running in a container (e.g. volume mount for config file or database)
- Optionally show metadata in enlarged photo view
- Error handling and logging improvements
- Support for varied image sources not just a file folder