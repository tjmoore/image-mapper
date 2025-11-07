using ImageMapper.Models;

namespace ImageMapper.Web.Client
{
    public class ImageItemFetcher(HttpClient httpClient)
    {
        /// <summary>
        /// Fetch list of available images with metadata
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ImageInfo>> Fetch(CancellationToken ct)
        {
            return (await httpClient.GetFromJsonAsync<List<ImageInfo>>("/api/images", ct)) ?? [];
        }


        /// <summary>
        /// Fetch image content streamed to the caller without buffering the entire response in memory.
        /// Caller is responsible for disposing the returned Stream when finished. 
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Stream?> FetchRawImageStream(string relativePath, CancellationToken ct)
        {
            var requestUrl = $"/api/images/raw/{Uri.EscapeDataString(relativePath)}";
            var response = await httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStreamAsync(ct);
        }
    }
}
