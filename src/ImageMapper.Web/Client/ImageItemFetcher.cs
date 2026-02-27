using ImageMapper.Models;

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
