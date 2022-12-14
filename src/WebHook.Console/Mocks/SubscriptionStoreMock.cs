using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client;
using WebHook.SubscriptionStore.Client.Models;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    private readonly string[] fakeUrls = new [] { "URL-1", "URL-2" };

   
    public IReadOnlyList<Subscription> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        throw new NotImplementedException();
    }

    public bool IsActive(int subscriptionId)
    {
        throw new NotImplementedException();
    }
}
