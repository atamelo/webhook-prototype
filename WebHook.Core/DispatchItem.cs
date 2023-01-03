using WebHook.Core.Events;

namespace WebHook.Core.Models;

public class DispatchItem
{
    public DispatchItem(Guid id, IEvent @event, int subscriptionId, string url)
    {
        Id = id;
        Event = @event;
        SubscriptionId = subscriptionId;
        Url = url;
    }

    public void IncreaseCount()
    {
        DispatchCount++;
    }

    public int DispatchCount { get; private set; }
    public Guid Id { get; }
    public int SubscriptionId { get; }
    public string Url { get; }
    public IEvent Event { get; }
}
