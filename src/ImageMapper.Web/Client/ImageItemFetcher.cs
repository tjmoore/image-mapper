using ImageMapper.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace ImageMapper.Web.Client
{
    public class ImageItemFetcher(HttpClient httpClient)
    {
        /// <summary>
        /// Fetch the total count of available images
        /// </summary>
        /// <returns>The total count of images</returns>
        public async Task<int> FetchImageCount(CancellationToken ct = default)
        {
            return await httpClient.GetFromJsonAsync<int>("/api/images/count", ct);
        }

        public async Task<CacheStatusInfo?> FetchCacheStatus(CancellationToken ct = default)
        {
            return await httpClient.GetFromJsonAsync<CacheStatusInfo>("/api/images/cache-status", ct);
        }

        public async IAsyncEnumerable<CacheStatusInfo?> StreamCacheStatus([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var response = await httpClient.GetAsync(
                "/api/images/cache-status/events",
                HttpCompletionOption.ResponseHeadersRead,
                ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);
            var dataBuilder = new StringBuilder();

            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                {
                    if (dataBuilder.Length > 0)
                    {
                        var status = ParseStatusData(dataBuilder.ToString());
                        if (status != null)
                        {
                            yield return status;
                        }
                    }

                    yield break;
                }

                if (line.Length == 0)
                {
                    if (dataBuilder.Length > 0)
                    {
                        var status = ParseStatusData(dataBuilder.ToString());
                        if (status != null)
                        {
                            yield return status;
                        }

                        dataBuilder.Clear();
                    }

                    continue;
                }

                if (line.StartsWith("data:", StringComparison.Ordinal))
                {
                    if (dataBuilder.Length > 0)
                    {
                        dataBuilder.Append('\n');
                    }

                    dataBuilder.Append(line[5..].TrimStart());
                }
            }
        }

        private static CacheStatusInfo? ParseStatusData(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return null;
            }

            return JsonSerializer.Deserialize<CacheStatusInfo>(
                jsonData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Fetch list of available images with metadata, streamed as async enumerable
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerable<ImageInfo?> Fetch(CancellationToken ct = default)
        {
            return httpClient.GetFromJsonAsAsyncEnumerable<ImageInfo>("/api/images", ct);
        }


        /// <summary>
        /// Fetch image content streamed to the caller without buffering the entire response in memory.
        /// Caller is responsible for disposing the returned Stream when finished. 
        /// </summary>
        /// <param name="id">The unique image ID</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Stream?> FetchRawImageStream(string id, CancellationToken ct)
        {
            var requestUrl = $"/api/images/raw/{id}";
            var response = await httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStreamAsync(ct);
        }
    }
}
