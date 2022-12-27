using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    private readonly string[] _fakeUrls;

    public SubscriptionStoreMock()
    {
        _fakeUrls = new[] { "URL-1", "URL-2" };
    }

    public IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        throw new NotImplementedException();
    }

    public bool IsActive(int subscriptionId)
    {
        throw new NotImplementedException();
    }
}
