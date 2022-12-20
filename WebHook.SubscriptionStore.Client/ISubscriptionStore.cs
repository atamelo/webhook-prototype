using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
    //TODO Pause vs Active
    IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    bool IsActive(int subscriptionId);
}