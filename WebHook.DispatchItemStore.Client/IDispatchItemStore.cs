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
public interface IDispatchItemStore
{
    void Put(DispatchItem item);

    void Remove(DispatchItem item);

    DispatchItem? GetNextOrDefault();
    void DelayRequeue(DispatchItem item, TimeSpan delay);
    IReadOnlyList<DispatchItem> GetNext(int count);
}