using Confluent.Kafka;
using Newtonsoft.Json;
using WebHook.Contracts.Events;
using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer;

public partial class ProducerLoop
{
    public class EventDeserilizer : IDeserializer<IEvent>
    {
        public IEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            try
            {
                string str = System.Text.Encoding.Default.GetString(data);
                return JsonConvert.DeserializeObject<IEvent>(str, new EventConverter());
            }
            catch (Exception e)
            {
                //TODO log fix etc
                return null;
            }
          
        }
    }
}
