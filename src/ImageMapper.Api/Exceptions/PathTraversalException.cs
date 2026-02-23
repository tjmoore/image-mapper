namespace ImageMapper.Api.Exceptions;

/// <summary>
/// Thrown when a path traversal attempt is detected.
/// </summary>
public class PathTraversalException : ArgumentException
{
    public PathTraversalException() : base("Path traversal detected") { }

    public PathTraversalException(string message) : base(message) { }

    public PathTraversalException(string message, Exception innerException)
        : base(message, innerException) { }

    public PathTraversalException(string message, string paramName)
        : base(message, paramName) { }
}
