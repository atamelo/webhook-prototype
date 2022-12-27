using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    public void Delete(string SubscriberId, int Id) => throw new NotImplementedException();

    public SubscriptionDTO Get(string SubscriberId, int Id) => throw new NotImplementedException();

    public IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent => throw new NotImplementedException();

    public bool IsActive(int subscriptionId) => throw new NotImplementedException();

    public void Save(SubscriptionDTO subscriptionDTO) => throw new NotImplementedException();
}
