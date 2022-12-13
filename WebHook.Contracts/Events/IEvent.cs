namespace WebHook.Contracts.Events;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/OwnerID, etc)
    public string TenantID { get; }
    public string EventID { get; }
    public string Payload { get; }
}
public record DummyEvent(string TenantID, string EventID, string Payload) : IEvent;