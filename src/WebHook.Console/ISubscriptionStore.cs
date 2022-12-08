using WebHook.Contracts.Events;

namespace WebHook.Console;

public interface ISubscriptionStore
{
    // TODO: async
    IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}

public class SubscriptionStoreMock : ISubscriptionStore
{
    public IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        return Array.Empty<string>();
    }
}
