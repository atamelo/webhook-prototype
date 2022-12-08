namespace WebHook.Contracts.Events;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/OwnerID, etc)
    string SubscriberID { get; }
}
