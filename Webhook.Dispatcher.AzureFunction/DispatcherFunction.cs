using System;
using System.Configuration;
using System.Text;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebHook.DispatchItemStore.Client;

namespace Webhook.Dispatcher.AzureFunction
{
    public class DispatcherFunction
    {
        private readonly ILogger _logger;

        public DispatcherFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DispatcherFunction>();
        }

        [Function("DispatcherFunction")]
        public async Task Run([QueueTrigger("dispatch", Connection = "AzureStorageQueueConnection")] string message)
        {
            
            DispatchItem item = ConvertToDispatchItem(message);


            //Functions have built in rety into host file so if failes item will be queued again after x time and then sent to DLQ after 3 attempts
            //all set in the host.json
            //TODO catch exception types etc, probably want logic on per error code basis
            await DispatchAsync(item);


        }
        public async Task DispatchAsync(DispatchItem item)
        {
            //TODO connection close, probably make a factory etc
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(item);
            StringContent stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            HttpResponseMessage result = await client.PostAsync("http://localhost:51084/Webhook", stringContent);
            result.EnsureSuccessStatusCode();
        }
        private static DispatchItem ConvertToDispatchItem(string stringData)
        {
            return JsonConvert.DeserializeObject<DispatchItem>(stringData, new EventConverter());
        }
    }
}
