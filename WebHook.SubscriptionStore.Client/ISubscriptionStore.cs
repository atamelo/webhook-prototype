using WebHook.Contracts.Events;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
    // TODO: async
    IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}
