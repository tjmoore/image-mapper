# TODO

- Support for folder patterns in image folder config
- Support for folder exclusion
- Fetch from backend in batches for performance? or is IAsyncEnumerable yielding one file at a time sufficient? (i.e. HTTP chunked streaming)
- Any benefit in websocket / HTTP/2 in backend for streaming metadata to frontend? chunked stream is still one open request.
	- Updates from backend when new images are added to the folder, or metadata changes. Would need to detect changes in backend though.
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- Caching. Memory and/or stored cache of processed image metadata to speed up subsequent loads and reduce processing on each request. Would need to detect changes however.
- UI improvements, filtering etc
- Optionally show metadata with enlarged photo view
- Error handling and logging improvements
- Container support
- Configurable map tile provider options?
- Support for varied image sources not just a file folder