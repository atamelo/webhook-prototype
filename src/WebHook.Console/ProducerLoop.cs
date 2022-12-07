using Confluent.Kafka;
using System.Text.RegularExpressions;

namespace WebHook.Console;

public class ProducerLoop
{
    private readonly ISubscriptionStore subscriptionStore;
    private readonly IDispatchItemStore dispatchItemStore;

    public ProducerLoop(
        ISubscriptionStore subscriptionStore,
        IDispatchItemStore dispatchItemStore)
    {
        this.subscriptionStore = subscriptionStore;
        this.dispatchItemStore = dispatchItemStore;
    }

    public void Start(EventLogConsumerConfig eventLogConsumerConfig, CancellationToken stopSignal, int commitBatchSize = 20)
    {
        // NOTE: this is kind of a global (i.e. across all the assigned partitions) batch size counter
        long eventsProcessed = 0;

        using IConsumer<string, IEvent> eventLogConsumer = this.CreateEventLogConsumer(eventLogConsumerConfig);

        while (stopSignal.IsCancellationRequested is false)
        {
            try
            {
                // NOTE: in Kafka client this is also a blocking call
                ConsumeResult<string, IEvent> record = eventLogConsumer.Consume(stopSignal);

                IReadOnlyList<string> urls = this.subscriptionStore.GetEndpointsFor(record.Message.Value, stopSignal);

                foreach (string url in urls)
                {
                    DispatchItem item = new(url, record.Message.Value);
                    dispatchItemStore.Put(item);
                }

                eventsProcessed++;

                bool batchSizeReached = eventsProcessed % commitBatchSize == 0;

                // in reality, when multiple Producer nodes are deployed, the batchSize
                // is only observed in scope of a single node. So, globally (or from the perspective
                // of the dispatchItemStore), the size of a batch can be somewhere from 1 to 
                // number_of_nodes * (batchSize - 1) items.
                if (batchSizeReached)
                {
                    // NOTE: first get a successful "commit" from the dispatch store
                    dispatchItemStore.PersistChanges();

                    // NOTE: an only then do a commit at the source
                    eventLogConsumer.Commit(record);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        // TODO: replace with loging
        System.Console.WriteLine("Producer loop aborted.");
    }

    protected virtual IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        // TODO: set up proper deserializer
        var builder = new ConsumerBuilder<string, IEvent>(config).SetValueDeserializer(null);
        IConsumer<string, IEvent> kafkaConsumer = builder.Build();

        kafkaConsumer.Subscribe(config.TopicNames);

        return kafkaConsumer;
    }

    public class EventLogConsumerConfig : ConsumerConfig
    {
        public IReadOnlyList<string> TopicNames { get; }

        public EventLogConsumerConfig(IReadOnlyList<string> topicNames) => this.TopicNames = topicNames;
    }
}