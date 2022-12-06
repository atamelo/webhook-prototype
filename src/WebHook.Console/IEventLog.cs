namespace WebHook.Console;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/OwnerID, etc)
    string SubscriberID { get; }
}

public readonly record struct EventEnvelope(IEvent Event, long Offset);

public interface IEventLog
{
    EventEnvelope PollForNext();

    void AcknowledgeUpTo(EventEnvelope envelope);
}
