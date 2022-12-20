using System.Collections.Concurrent;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client.AzureQueueStorage
{
    public class AzureDispatchItemStore : IDispatchItemStore
    {
        private readonly QueueClient queue;

        //Concurrent since remove is taking place on different threads
        private readonly ConcurrentDictionary<Guid, QueueMessage> inProgressMessages;

        public AzureDispatchItemStore()
        {
            inProgressMessages = new();
            //TODO make from config
            queue = new(
                "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;",
                "dispatch",
                //FOR AZURE FUNCTIONS THEY NEED 64 ENCODING
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                }
                       );

            queue.CreateIfNotExists();
        }

        public void Enqueue(DispatchItem item, TimeSpan delay)
        {
            QueueMessage message = inProgressMessages[item.Id];
            queue.UpdateMessage(message.MessageId, message.PopReceipt, message.Body, delay);
        }

        public IReadOnlyList<DispatchItem> GetNext(int maxMessages)
        {
            maxMessages = Math.Min(32, maxMessages);

            //TODO how long is long enough? configurable dynaimc?
            QueueMessage[] message = queue.ReceiveMessages(maxMessages: maxMessages, TimeSpan.FromSeconds(30));

            if (message is null)
            {
                return Array.Empty<DispatchItem>();
            }

            return message.Select(ConvertToDispatchItem).ToList();
        }

        public DispatchItem? GetNextOrDefault()
        {
            QueueMessage message = queue.ReceiveMessage(TimeSpan.FromSeconds(30));

            if (message is null)
            {
                return null;
            }

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

        public void Enqueue(DispatchItem item)
        {
            string stringItem = JsonConvert.SerializeObject(item, Formatting.None);
            queue.SendMessage(stringItem);
        }

        public void Remove(DispatchItem item)
        {
            QueueMessage message = inProgressMessages[item.Id];

            //TODO manage responses / errors
            queue.DeleteMessage(message.MessageId, message.PopReceipt);
            inProgressMessages.TryRemove(new KeyValuePair<Guid, QueueMessage>(item.Id, message));
        }
    }
}