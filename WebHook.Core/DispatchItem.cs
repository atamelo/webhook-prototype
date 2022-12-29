using WebHook.Core.Events;

namespace WebHook.Core.Models;

public class DispatchItem
{
    public DispatchItem(Guid id, IEvent @event, int subscriptionId)
    {
        Id = id;
        Event = @event;
        SubscriptionId = subscriptionId;
    }

    public int DispatchCount { get; set; }
    public Guid Id { get; set; }
    public int SubscriptionId { get; set; }
    public IEvent Event { get; set; }
}
