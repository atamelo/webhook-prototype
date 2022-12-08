using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using WebHook.Contracts.Events;
using WebHook.DispatchItemStore.Client;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer;

public class ProducerLoop
{
    private readonly ISubscriptionStore subscriptionStore;
    private readonly IDispatchItemStore dispatchItemStore;
    private readonly ILogger<ProducerLoop> logger;

    public ProducerLoop(
        ISubscriptionStore subscriptionStore,
        IDispatchItemStore dispatchItemStore,
        ILogger<ProducerLoop> logger)
    {
        this.subscriptionStore = subscriptionStore;
        this.dispatchItemStore = dispatchItemStore;
        this.logger = logger;
    }

    public void Start(EventLogConsumerConfig eventLogConsumerConfig, CancellationToken stopSignal, int commitBatchSize = 20)
    {
        // NOTE: this is kind of a global (i.e. across all the assigned partitions) batch size counter
        long eventsProcessed = 0;

        using IConsumer<string, IEvent> eventLogConsumer = CreateEventLogConsumer(eventLogConsumerConfig);

        logger.LogInformation("Producer loop starting...");

        while (stopSignal.IsCancellationRequested is false)
        {
            try
            {
                // NOTE: in Kafka client this is also a blocking call
                ConsumeResult<string, IEvent> record = eventLogConsumer.Consume(stopSignal);

                // TODO: add extensive logging

                IReadOnlyList<string> urls = subscriptionStore.GetEndpointsFor(record.Message.Value, stopSignal);

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

        logger.LogInformation("Producer loop aborted.");
    }

    protected virtual IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        logger.LogInformation("Creating kafka consumer...");

        // TODO: set up proper deserializer!!
        var builder = new ConsumerBuilder<string, IEvent>(config).SetValueDeserializer(null);
        IConsumer<string, IEvent> kafkaConsumer = builder.Build();

        kafkaConsumer.Subscribe(config.TopicNames);

        return kafkaConsumer;
    }

    public class EventLogConsumerConfig : ConsumerConfig
    {
        public IReadOnlyList<string> TopicNames { get; }

        public EventLogConsumerConfig(IReadOnlyList<string> topicNames) => TopicNames = topicNames;
    }

    public class ProducerConfig
    {
        public int CommitBatchSize { get; } = 50;
    }
}
