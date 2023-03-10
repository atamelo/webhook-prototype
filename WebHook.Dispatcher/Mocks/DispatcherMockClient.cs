using System.Text;
using Newtonsoft.Json;
using WebHook.Core.Models;

public class DispatcherMockClient : IDispatcherClient
{
    private readonly HttpClient _client;

    public DispatcherMockClient()
    {
        _client = new HttpClient();
    }

    public async Task DispatchAsync(DispatchItem item)
    {
        item.IncreaseCount();
        string json = JsonConvert.SerializeObject(item);
        StringContent stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
        HttpResponseMessage result = await _client.PostAsync(item.Url, stringContent);
        result.EnsureSuccessStatusCode();
        return;
    }
}
