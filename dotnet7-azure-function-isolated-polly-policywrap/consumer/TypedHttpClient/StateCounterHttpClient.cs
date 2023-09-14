namespace consumer.TypedHttpClient
{
    public class StateCounterHttpClient
    {
        private readonly HttpClient _httpClient;

        public StateCounterHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetStateCounter(string endpoint) =>
            await _httpClient.GetStringAsync(endpoint);
    }
}