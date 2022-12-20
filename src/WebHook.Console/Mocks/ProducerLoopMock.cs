using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using WebHook.Core.Events;
using WebHook.DispatchItemStore.Client;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class ProducerLoopMock : ProducerLoop
{
    private readonly BlockingCollection<IEvent> source;

    public ProducerLoopMock(
        ISubscriptionStore subscriptionStore,
        IDispatchItemStore dispatchItemStore,
        ILogger<ProducerLoop> logger,
        BlockingCollection<IEvent> source) : base(subscriptionStore, dispatchItemStore, logger)
    {
        this.source = source;
    }

    protected override IConsumer<string, IEvent> CreateEventLogConsumer(EventLogConsumerConfig config)
    {
        return new KafkaConsumerMock(this.source);
    }

    private class KafkaConsumerMock : IConsumer<string, IEvent>
    {
        private readonly BlockingCollection<IEvent> source;

        public string MemberId => throw new NotImplementedException();

        public List<TopicPartition> Assignment => throw new NotImplementedException();

        public List<string> Subscription => throw new NotImplementedException();

        public IConsumerGroupMetadata ConsumerGroupMetadata => throw new NotImplementedException();

        public Handle Handle => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public KafkaConsumerMock(BlockingCollection<IEvent> source)
        {
            this.source = source;
        }

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
            IEvent @event = source.Take(cancellationToken);

            return new() { Message = new() { Key = "dummyKey", Value = @event } };
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