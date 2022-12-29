using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
    void DeleteSubscription(int Id);

    SubscriptionDto? GetSubscriptionFor(int Id);

    IReadOnlyCollection<SubscriptionDto> GetSubscriptionsFor(string SubscriberId);

    IReadOnlyList<SubscriptionDto> GetActiveSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    bool IsActive(int subscriptionId);

    int CreateSubscription(SubscriptionDto subscriptionDto);

    void UpdateSubscription(SubscriptionDto subscriptionDto);
}
