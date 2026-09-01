using ImageMapper.Models;

namespace ImageMapper.Services
{
    public interface ICacheActivityStatus
    {
        /// <summary>
        /// Gets a value indicating whether caching activities are currently active.
        /// </summary>
        public bool IsCaching { get; }

        /// <summary>
        /// Gets the current status of caching activities.
        /// </summary>
        /// <returns>A <see cref="CacheStatusInfo"/> object representing the current status of caching activities.</returns>
        public CacheStatusInfo GetStatus();
        
        /// <summary>
        /// Marks the start of caching activities.
        /// </summary>
        /// <param name="totalFileCount">The total number of files to be processed.</param>
        public void MarkCachingStarted(int totalFileCount);

        /// <summary>
        /// Updates the progress of caching activities by setting the number of processed files and the total number of files to be processed.
        /// </summary>
        /// <param name="processedFileCount">The number of files that have been processed.</param>
        /// <param name="totalFileCount">The total number of files to be processed.</param>
        public void UpdateProgress(int processedFileCount, int totalFileCount);

        /// <summary>
        /// Marks the stop of caching activities. If there are no remaining active cache operations, it publishes the updated status indicating that caching has stopped.
        /// </summary>
        public void MarkCachingStopped();

        /// <summary>
        /// Streams the current status of caching activities to subscribers. Each subscriber receives updates whenever the status changes.
        /// The method returns an asynchronous enumerable that can be consumed to receive status updates in real-time.
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
        /// <returns>An asynchronous enumerable of <see cref="CacheStatusInfo"/> objects representing the status updates.</returns>
        public IAsyncEnumerable<CacheStatusInfo> StreamStatuses(CancellationToken ct = default);
    }
}