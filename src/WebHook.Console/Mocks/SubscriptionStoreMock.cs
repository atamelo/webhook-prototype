using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    private readonly string[] fakeUrls = new [] { "URL-1", "URL-2" };

    public IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => this.fakeUrls;
}
