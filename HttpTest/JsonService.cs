namespace HttpTest
{
    public interface IJsonService
    {
        Task<string> GetJsonAsync(string url);
    }
    public class JsonService : IJsonService
    {
        private readonly HttpClient _httpClient;

        public JsonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetJsonAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
