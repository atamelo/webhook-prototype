namespace WebHook.Core.Events;

public class DummyEvent : IEvent
{
    public DummyEvent(string subscriberId, string eventID, string payload)
    {
        SubscriberId = subscriberId;
        EventId = eventID;
        Payload = payload;
    }

    public DummyEvent()
    {
    }

    public string SubscriberId { get; set; }
    public string EventId { get; set; }
    public string Payload { get; set; }
}
