using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client.Models;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
   
    //TODO Pause vs Active
    IReadOnlyList<Subscription> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
    bool IsActive(int subscriptionId);
}
