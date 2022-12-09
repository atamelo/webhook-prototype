namespace WebHook.Contracts.Events;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/OwnerID, etc)
    public string SubscriberID { get; }
}
public record DummyEvent(string SubscriberID) : IEvent;