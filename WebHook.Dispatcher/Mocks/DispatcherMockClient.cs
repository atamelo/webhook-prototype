using System.Text;
using Newtonsoft.Json;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;
using WebHook.SubscriptionSotre.Client.Models;

public class DispatcherMockClient : IDispatcherClient
{
    private readonly HttpClient client;

    public DispatcherMockClient()
    {
        client = new HttpClient();
    }

    public async Task DispatchAsync(DispatchItem item)
    {
        string json = JsonConvert.SerializeObject(item);
        StringContent stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
        HttpResponseMessage result = await client.PostAsync("http://localhost:51084/Webhook", stringContent);
        result.EnsureSuccessStatusCode();
        return;
    }
}