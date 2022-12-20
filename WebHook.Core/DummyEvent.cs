namespace WebHook.Core.Events;

public class DummyEvent : IEvent
{
    public DummyEvent(string tenantID, string eventID, string payload)
    {
        SubscriberId = tenantID;
        EventId = eventID;
        Payload = payload;
    }

    public string SubscriberId { get; }
    public string EventId { get; }
    public string Payload { get; }
}