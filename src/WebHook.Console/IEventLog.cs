﻿namespace WebHook.Console;

public interface IEvent
{
    // TODO: figure out a better name (TenantID/SubscriberID, etc)
    string OwnerID { get; }
}

public readonly record struct EventEnvelope(IEvent Event, long Offset);

public interface IEventLog
{
    EventEnvelope PollForNext();

    void AcknowledgeUpTo(long offset);
}
