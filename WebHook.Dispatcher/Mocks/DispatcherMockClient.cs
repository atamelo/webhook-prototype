using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using WebHook.DispatchItemStore.Client;
using static System.Net.WebRequestMethods;

public class DispatcherMockClient : IDispatcherClient
{
    private HttpClient client;

    public DispatcherMockClient()
    {
        client = new HttpClient();
    }
    public async Task DispatchAsync(DispatchItem item)
    {
        string json = JsonConvert.SerializeObject(item);
        var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
        HttpResponseMessage result = await client.PostAsync("http://localhost:51084/Webhook", stringContent);
        result.EnsureSuccessStatusCode();
        return;
    }
}
