namespace ImageMapper.RazorLib.Interops
{
    /// <summary>
    /// Converts images in formats not supported by browsers into a browser-displayable format.
    /// </summary>
    internal interface IImageConverter
    {
        /// <summary>
        /// Determines whether this converter handles the given image.
        /// </summary>
        bool CanConvert(string? contentType, string? fileName);

        /// <summary>
        /// Converts the source image to JPEG. The caller retains ownership of the source stream.
        /// The returned stream is positioned at the start and owned by the caller.
        /// </summary>
        Task<Stream> ConvertToJpegAsync(Stream source, CancellationToken cancellationToken = default);
    }
}
