using WebHook.Core.Events;

namespace WebHook.Core.Models;

public class DispatchItem
{
    public DispatchItem(Guid id, IEvent @event)
    {
        Id = id;
        Event = @event;
    }

    public int DispatchCount { get; set; }
    public Guid Id { get; set; }
    public IEvent Event { get; set; }
}