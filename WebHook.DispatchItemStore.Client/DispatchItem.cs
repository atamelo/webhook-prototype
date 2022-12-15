using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client.Models;

namespace WebHook.DispatchItemStore.Client;

public class DispatchItem
{
    public DispatchItem(Guid id, Subscription subscription, IEvent @event)
    {
        Id = id;
        Subscription = subscription;
        Event = @event;
    }

    public int DispatchCount { get; set; }
    public Guid Id { get; set; }
    public Subscription Subscription { get; set; }
    public IEvent Event { get; set; }
}
