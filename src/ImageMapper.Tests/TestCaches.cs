using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ImageMapper.Tests
{
    internal static class TestCaches
    {
        /// <summary>
        /// Creates a HybridCache, optionally backed by a shared distributed cache standing in for Redis
        /// </summary>
        public static HybridCache Create(IDistributedCache? distributedCache = null)
        {
            var services = new ServiceCollection();
            if (distributedCache != null)
                services.AddSingleton(distributedCache);

            services.AddHybridCache();
            return services.BuildServiceProvider().GetRequiredService<HybridCache>();
        }

        // HybridCache ignores MemoryDistributedCache as a second level, so use a minimal dictionary-backed stand-in for Redis
        public static IDistributedCache CreateDistributedCache() => new DictionaryDistributedCache();

        private sealed class DictionaryDistributedCache : IDistributedCache
        {
            private readonly ConcurrentDictionary<string, byte[]> _store = [];

            public byte[]? Get(string key) => _store.GetValueOrDefault(key);

            public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _store[key] = value;

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            {
                Set(key, value, options);
                return Task.CompletedTask;
            }

            public void Refresh(string key) { }

            public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

            public void Remove(string key) => _store.TryRemove(key, out _);

            public Task RemoveAsync(string key, CancellationToken token = default)
            {
                Remove(key);
                return Task.CompletedTask;
            }
        }
    }
}
