using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using WebHook.DispatchItemStore.Client.Redis;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace WebHook.DispatchItemStore.Client.AzureQueueStorage
{
    public class AzureDispatchItemStore : IDispatchItemStore
    {
        QueueClient queue;
        ConcurrentDictionary<Guid, QueueMessage> inProgressMessages = new();
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

        public IReadOnlyList<DispatchItem> GetNext(int maxMessages)
        {
            if(maxMessages>32) maxMessages= 32;

            //TODO how long is long enough? configurable dynaimc?
            QueueMessage[] message = queue.ReceiveMessages(maxMessages: maxMessages, TimeSpan.FromSeconds(30));

            if (message is null) return new List<DispatchItem>();

            return message.Select(m => ConvertToDispatchItem(m)).ToList();
        }

        public DispatchItem? GetNextOrDefault()
        {
            QueueMessage message = queue.ReceiveMessage(TimeSpan.FromSeconds(30));

            if (message is null) return null;

            DispatchItem returnItem = ConvertToDispatchItem(message);

            return returnItem;
        }

        private DispatchItem ConvertToDispatchItem(QueueMessage message)
        {
            string stringData = message.Body.ToString();
      
            DispatchItem returnItem = JsonConvert.DeserializeObject<DispatchItem>(stringData, new EventConverter());
            if (inProgressMessages.ContainsKey(returnItem.Id)) 
            {
                inProgressMessages[returnItem.Id] = message;
            }
            else
            {
                inProgressMessages.TryAdd(returnItem.Id, message);
            }
           
            return returnItem;
        }

        public void Put(DispatchItem item)
        {
            string stringItem = JsonConvert.SerializeObject(item, Formatting.None);
            queue.SendMessage(stringItem);
        }

        public void Remove(DispatchItem item)
        {
            QueueMessage message = inProgressMessages[item.Id];

            //TODO manage responses / errors
            queue.DeleteMessage(message.MessageId, message.PopReceipt);
            bool removed = false;
            inProgressMessages.TryRemove(new KeyValuePair<Guid, QueueMessage>(item.Id, message));

        }
    }
}