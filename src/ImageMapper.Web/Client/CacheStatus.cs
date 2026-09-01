using ImageMapper.Models;
using ImageMapper.Services;

namespace ImageMapper.Web.Client
{
    public class CacheStatus(ICacheActivityStatus cacheActivityStatus)
    {
        /// <summary>
        /// Fetches the current cache status information.
        /// </summary>
        /// <returns>The current cache status information</returns>
        public CacheStatusInfo FetchCacheStatus() => cacheActivityStatus.GetStatus();

        /// <summary>
        /// Streams the cache status information as an asynchronous enumerable.
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>An asynchronous enumerable of cache status information</returns>
        public IAsyncEnumerable<CacheStatusInfo?> StreamCacheStatus(CancellationToken ct = default)
        {
            return cacheActivityStatus.StreamStatuses(ct);
        }
    }
}
