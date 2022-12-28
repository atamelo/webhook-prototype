using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
    void Delete(string SubscriberId, int Id);

    SubscriptionDto Find(string SubscriberId, int Id);

    IReadOnlyCollection<SubscriptionDto> GetAll(string SubscriberId);

    IReadOnlyList<SubscriptionDto> GetActiveSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    bool IsActive(int subscriptionId);

    void Save(SubscriptionDto subscriptionDto);
}
