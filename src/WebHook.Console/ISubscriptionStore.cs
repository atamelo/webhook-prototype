namespace WebHook.Console;

public interface ISubscriptionStore
{
    IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event) where TEvent : IEvent;
}
