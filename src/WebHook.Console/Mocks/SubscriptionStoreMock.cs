using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer.Mocks;

public class SubscriptionStoreMock : ISubscriptionStore
{
    public void Delete(string SubscriberId, int Id) => throw new NotImplementedException();

    public SubscriptionDto Find(string SubscriberId, int Id) => throw new NotImplementedException();

    public IReadOnlyCollection<SubscriptionDto> GetAll(string SubscriberId) => throw new NotImplementedException();

    public IReadOnlyList<SubscriptionDto> GetActiveSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent => throw new NotImplementedException();

    public bool IsActive(int subscriptionId) => throw new NotImplementedException();

    public void Save(SubscriptionDto subscriptionDto) => throw new NotImplementedException();
}
