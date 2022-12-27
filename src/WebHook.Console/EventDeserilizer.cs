using Confluent.Kafka;
using Newtonsoft.Json;
using WebHook.Core.Events;
using WebHook.DispatchItemQueue.Client;

namespace WebHook.Producer;

public partial class ProducerLoop
{
    public class EventDeserilizer : IDeserializer<IEvent>
    {
        public IEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            try {
                string str = System.Text.Encoding.Default.GetString(data);
                return JsonConvert.DeserializeObject<IEvent>(str, new EventConverter());
            }
            catch (Exception) {
                //TODO log fix etc
                return null;
            }
        }
    }
}