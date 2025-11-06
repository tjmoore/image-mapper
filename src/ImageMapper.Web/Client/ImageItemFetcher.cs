using ImageMapper.Models;

namespace ImageMapper.Web.Client
{
    public class ImageItemFetcher(HttpClient httpClient)
    {
        public async Task<IEnumerable<ImageInfo>> Fetch()
        {
            return (await httpClient.GetFromJsonAsync<List<ImageInfo>>("/api/images")) ?? [];
        }
    }
}
