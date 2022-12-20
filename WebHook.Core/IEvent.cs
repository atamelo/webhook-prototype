namespace WebHook.Core.Events;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/OwnerID, etc)
    public string SubscriberId { get; }

    public string EventId { get; }

    public string Payload { get; }
}