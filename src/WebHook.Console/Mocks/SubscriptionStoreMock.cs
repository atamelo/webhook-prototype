using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    public IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        return Array.Empty<string>();
    }
}
