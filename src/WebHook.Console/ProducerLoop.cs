using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using WebHook.Core.Events;
using WebHook.Core.Models;
using WebHook.DispatchItemQueue.Client;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer;

public partial class ProducerLoop
{
    private readonly ISubscriptionStore _subscriptionStore;
    private readonly IDispatchItemQueue _dispatchItemQueue;
    private readonly ILogger<ProducerLoop> _logger;

    public ProducerLoop(
        ISubscriptionStore subscriptionStore,
        IDispatchItemQueue dispatchItemQueue,
        ILogger<ProducerLoop> logger)
    {
        _subscriptionStore = subscriptionStore;
        _dispatchItemQueue = dispatchItemQueue;
        _logger = logger;
    }

    public void Start(EventLogConsumerConfig eventLogConsumerConfig, CancellationToken stopSignal, int commitBatchSize = 20)
    {
        // NOTE: this is kind of a global (i.e. across all the assigned partitions) batch size counter
        long eventsProcessed = 0;

        using IConsumer<string, IEvent> eventLogConsumer = CreateEventLogConsumer(eventLogConsumerConfig);

        _logger.LogInformation("Producer loop starting...");

        while (stopSignal.IsCancellationRequested is false) {
            try {
                // NOTE: in Kafka client this is also a blocking call
                ConsumeResult<string, IEvent> record = eventLogConsumer.Consume(stopSignal);

                if (record?.Message?.Value is null) {
                    continue;
                }

                // TODO: add extensive logging

                IReadOnlyList<SubscriptionDto> subscriptions = _subscriptionStore.GetActiveSubscriptionsFor(record.Message.Value, stopSignal);

                foreach (SubscriptionDto subscription in subscriptions) {
                    DispatchItem item = new(Guid.NewGuid(), record.Message.Value, subscription.Id, subscription.Url);
                    _dispatchItemQueue.EnqueueAsync(item).Wait();
                }

                eventsProcessed++;

                bool batchSizeReached = eventsProcessed % commitBatchSize == 0;

                // in reality, when multiple Producer nodes are deployed, the batchSize
                // is only observed in scope of a single node. So, globally (or from the perspective
                // of the dispatchItemQueue), the size of a batch can be somewhere from 1 to
                // number_of_nodes * (batchSize - 1) items.
                if (batchSizeReached) {
                    // NOTE: an only then do a commit at the source
                    eventLogConsumer.Commit(record);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        _logger.LogInformation("Producer loop aborted.");
    }

    protected virtual IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        _logger.LogInformation("Creating kafka consumer...");

        // TODO: set up proper deserializer!!
        ConsumerBuilder<string, IEvent> builder = new ConsumerBuilder<string, IEvent>(config).SetValueDeserializer(new EventDeserilizer());
        IConsumer<string, IEvent> kafkaConsumer = builder.Build();

        kafkaConsumer.Subscribe(config.TopicNames);

        return kafkaConsumer;
    }
}
