namespace ImageMapper.Services
{
    // Based on sample https://learn.microsoft.com/en-us/dotnet/core/extensions/caching#photo-service-scenario

    /// <summary>
    /// A signal mechanism for coordinating cache access between producers and consumers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CacheSignal<T>
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Exposes a <see cref="Task"/> that represents the asynchronous wait operation.
        /// When signaled (consumer calls <see cref="Release"/>), the 
        /// <see cref="Task.Status"/> is set as <see cref="TaskStatus.RanToCompletion"/>.
        /// </summary>
        public Task WaitAsync(CancellationToken ct = default) => _semaphore.WaitAsync(ct);

        /// <summary>
        /// Exposes the ability to signal the release of the <see cref="WaitAsync"/>'s operation.
        /// Callers who were waiting, will be able to continue.
        /// </summary>
        public void Release() => _semaphore.Release();
    }
}
