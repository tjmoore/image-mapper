# TODO

- Support for other formats including HIEF/HEIC and various raw image formats and/or anything MetadataExtractor supports (see https://github.com/drewnoakes/metadata-extractor-dotnet/blob/main/MetadataExtractor.Tools.FileProcessor/FileHandlerBase.cs)
- Support for folder patterns in image folder config
- Support for folder exclusion
- Fetch from backend in batches for performance instead of or as well as front-end render batching
- CSS improvements - SASS and/or Blazor CSS isolation. Not embedded in JS
- Caching. Memory and/or stored cache of processed image metadata to speed up subsequent loads and reduce processing on each request. Would need to detect changes however.
- UI improvements, filtering etc
- Optionally show metadata with enlarged photo view
- Error handling and logging improvements
- Container support
- Configurable map tile provider options?
- Support for varied image sources not just a file folder