namespace WebHook.Contracts.Events;

public class DummyEvent : IEvent
{
    public DummyEvent(string tenantID, string eventID, string payload)
    {
        TenantID = tenantID;
        EventID = eventID;
        Payload = payload;
    }

    public string TenantID { get; }
    public string EventID { get; }
    public string Payload { get; }
}