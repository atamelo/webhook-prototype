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
        public async Task Run(
            [Microsoft.Azure.Functions.Worker.QueueTrigger("dispatch", Connection = "AzureStorageQueueConnection")] string message,string PopReceipt, string id)
        {
            
            
            DispatchItem item = ConvertToDispatchItem(message);

            //TODO catch exception types etc, probably want logic on per error code basis
            await DispatchAsync(item);


        }
        public async Task DispatchAsync(DispatchItem item)
        {
            //TODO connection close etc
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(item);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            HttpResponseMessage result = await client.PostAsync("http://localhost:51084/Webhook", stringContent);
            result.EnsureSuccessStatusCode();
            return;
        }
        private static DispatchItem ConvertToDispatchItem(string stringData)
        {
            return JsonConvert.DeserializeObject<DispatchItem>(stringData, new EventConverter());
        }
    }
}
