namespace WebHook.Console;

public interface ISubscriptionStore
{
    // TODO: async
    IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}
