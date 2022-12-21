using System.Collections.Concurrent;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client.AzureQueueStorage
{
    public class AzureDispatchItemStore : IDispatchItemStore
    {
        private readonly QueueClient _queue;

        //Concurrent since remove is taking place on different threads
        private readonly ConcurrentDictionary<Guid, QueueMessage> _inProgressMessages;

        public AzureDispatchItemStore()
        {
            _inProgressMessages = new();
            //TODO make from config
            _queue = new(
                "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;",
                "dispatch",
                //FOR AZURE FUNCTIONS THEY NEED 64 ENCODING
                new QueueClientOptions {
                    MessageEncoding = QueueMessageEncoding.Base64
                }
                        );

            _queue.CreateIfNotExists();
        }

        public void Enqueue(DispatchItem item, TimeSpan delay)
        {
            QueueMessage message = _inProgressMessages[item.Id];
            _queue.UpdateMessage(message.MessageId, message.PopReceipt, message.Body, delay);
        }

        public IReadOnlyList<DispatchItem> GetNext(int maxMessages)
        {
            maxMessages = Math.Min(32, maxMessages);

            //TODO how long is long enough? configurable dynaimc?
            QueueMessage[] message = _queue.ReceiveMessages(maxMessages: maxMessages, TimeSpan.FromSeconds(30));

            if (message is null) {
                return Array.Empty<DispatchItem>();
            }

            return message.Select(ConvertToDispatchItem).ToList();
        }

        public DispatchItem? GetNextOrDefault()
        {
            QueueMessage message = _queue.ReceiveMessage(TimeSpan.FromSeconds(30));

            if (message is null) {
                return null;
            }

            DispatchItem returnItem = ConvertToDispatchItem(message);

            return returnItem;
        }

        private DispatchItem ConvertToDispatchItem(QueueMessage message)
        {
            string stringData = message.Body.ToString();

            DispatchItem returnItem = JsonConvert.DeserializeObject<DispatchItem>(stringData, new EventConverter());
            if (_inProgressMessages.ContainsKey(returnItem.Id)) {
                _inProgressMessages[returnItem.Id] = message;
            }
            else {
                _inProgressMessages.TryAdd(returnItem.Id, message);
            }

            return returnItem;
        }

        public void Enqueue(DispatchItem item)
        {
            string stringItem = JsonConvert.SerializeObject(item, Formatting.None);
            _queue.SendMessage(stringItem);
        }

        public void Remove(DispatchItem item)
        {
            QueueMessage message = _inProgressMessages[item.Id];

            //TODO manage responses / errors
            _queue.DeleteMessage(message.MessageId, message.PopReceipt);
            _inProgressMessages.TryRemove(new KeyValuePair<Guid, QueueMessage>(item.Id, message));
        }
    }
}
