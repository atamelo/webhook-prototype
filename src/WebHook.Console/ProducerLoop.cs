using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace WebHook.Console;

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

        using IConsumer<string, IEvent> eventLogConsumer = this.CreateEventLogConsumer(eventLogConsumerConfig);

        this.logger.LogInformation("Producer loop starting...");

        while (stopSignal.IsCancellationRequested is false)
        {
            try
            {
                // NOTE: in Kafka client this is also a blocking call
                ConsumeResult<string, IEvent> record = eventLogConsumer.Consume(stopSignal);

                // TODO: add extensive logging

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

        this.logger.LogInformation("Producer loop aborted.");
    }

    protected virtual IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        this.logger.LogInformation("Creating kafka consumer...");

        // TODO: set up proper deserializer!!
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

    public class ProducerConfig
    {
        public int CommitBatchSize { get; } = 50;
    }
}

public class ProducerLoopMock : ProducerLoop
{
    public ProducerLoopMock(
        ISubscriptionStore subscriptionStore,
        IDispatchItemStore dispatchItemStore,
        ILogger<ProducerLoop> logger) : base(subscriptionStore, dispatchItemStore, logger)
    {
    }

    protected override IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        return new KafkaConsumerMock();
    }

    private record DummyEvent(string SubscriberID) : IEvent;

    private class KafkaConsumerMock : IConsumer<string, IEvent>
    {
        public string MemberId => throw new NotImplementedException();

        public List<TopicPartition> Assignment => throw new NotImplementedException();

        public List<string> Subscription => throw new NotImplementedException();

        public IConsumerGroupMetadata ConsumerGroupMetadata => throw new NotImplementedException();

        public Handle Handle => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public int AddBrokers(string brokers)
        {
            throw new NotImplementedException();
        }

        public void Assign(TopicPartition partition)
        {
            throw new NotImplementedException();
        }

        public void Assign(TopicPartitionOffset partition)
        {
            throw new NotImplementedException();
        }

        public void Assign(IEnumerable<TopicPartitionOffset> partitions)
        {
            throw new NotImplementedException();
        }

        public void Assign(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
        }

        public List<TopicPartitionOffset> Commit()
        {
            throw new NotImplementedException();
        }

        public void Commit(IEnumerable<TopicPartitionOffset> offsets)
        {
            throw new NotImplementedException();
        }

        public void Commit(ConsumeResult<string, IEvent> result)
        {
        }

        public List<TopicPartitionOffset> Committed(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public ConsumeResult<string, IEvent> Consume(int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        public ConsumeResult<string, IEvent> Consume(CancellationToken cancellationToken = default)
        {
            return new () { Message = new() { Key = "dummyKey", Value = new DummyEvent("dummySubscriber") } };
        }

        public ConsumeResult<string, IEvent> Consume(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
        {
            throw new NotImplementedException();
        }

        public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
        {
            throw new NotImplementedException();
        }

        public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void Pause(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public Offset Position(TopicPartition partition)
        {
            throw new NotImplementedException();
        }

        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void Resume(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void Seek(TopicPartitionOffset tpo)
        {
            throw new NotImplementedException();
        }

        public void StoreOffset(ConsumeResult<string, IEvent> result)
        {
            throw new NotImplementedException();
        }

        public void StoreOffset(TopicPartitionOffset offset)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string topic)
        {
            throw new NotImplementedException();
        }

        public void Unassign()
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}