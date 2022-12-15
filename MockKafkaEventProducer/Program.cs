

using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebHook.Contracts.Events;

var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

// If serializers are not specified, default serializers from
// `Confluent.Kafka.Serializers` will be automatically used where
// available. Note: by default strings are encoded as UTF8.
using (var p = new ProducerBuilder<Null, string>(config).Build())
{
    Random rand = new();
  
    while (true)
    {
        try
        {
            int tenantId = rand.Next(1, 100);
            int eventId = rand.Next(1, 10);
            DeliveryResult<Null, string> dr = await ProduceEvent(p,tenantId, eventId);
           
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }
   
}

static async Task<DeliveryResult<Null, string>> ProduceEvent(IProducer<Null, string> p,int tenantId, int eventId)
{

    DummyEvent @event = new(
        tenantId.ToString(), 
        eventId.ToString(), 
        JObject.FromObject(new { data = "im a real event" }).ToString()
        );
    return await p.ProduceAsync("test-topic", new Message<Null, string> { Value = JsonConvert.SerializeObject(@event) });
}