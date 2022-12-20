using Confluent.Kafka;

namespace WebHook.Producer;

public partial class ProducerLoop
{
    public class EventLogConsumerConfig : ConsumerConfig
    {
        public IReadOnlyList<string> TopicNames { get; }

        public EventLogConsumerConfig(IReadOnlyList<string> topicNames) => TopicNames = topicNames;
    }
}