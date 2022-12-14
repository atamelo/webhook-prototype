using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using WebHook.DispatchItemStore.Client.Redis;
using Newtonsoft.Json;

namespace WebHook.DispatchItemStore.Client.AzureQueueStorage
{
    public class AzureDispatchItemStore : IDispatchItemStore
    {
        QueueClient queue;
        Dictionary<Guid, QueueMessage> inProgressMessages = new();
        public AzureDispatchItemStore()
        {
            //TODO Config
            queue = new(
                "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;",
                "dispatch"
            );
            queue.CreateIfNotExists();
        }
        public void DelayRequeue(DispatchItem item, TimeSpan delay)
        {
            QueueMessage message = inProgressMessages[item.Id];
            queue.UpdateMessage(message.MessageId, message.PopReceipt, message.Body, delay);
        }

        public DispatchItem? GetNextOrDefault()
        {
            QueueMessage message = queue.ReceiveMessage(TimeSpan.FromSeconds(30));
            
            if (message is null) return null;

            string stringData = message.Body.ToString();
            JObject obj = JObject.Parse(stringData);
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new EventConverter());
            DispatchItem returnItem = obj.ToObject<DispatchItem>(jsonSerializer);
            inProgressMessages.Add(returnItem.Id, message);
            return returnItem;
        }

        public void Put(DispatchItem item)
        {
            string stringItem = JObject.FromObject(item).ToString();
            queue.SendMessage(stringItem);
        }

        public void Remove(DispatchItem item)
        {
            QueueMessage message = inProgressMessages[item.Id];
            queue.DeleteMessage(message.MessageId, message.PopReceipt);
        }
    }
}