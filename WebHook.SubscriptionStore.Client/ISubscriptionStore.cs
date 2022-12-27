using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client;

public interface ISubscriptionStore
{
    void Delete(string SubscriberId, int Id);

    SubscriptionDTO Get(string SubscriberId, int Id);

    IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    bool IsActive(int subscriptionId);

    void Save(SubscriptionDTO subscriptionDTO);
}
