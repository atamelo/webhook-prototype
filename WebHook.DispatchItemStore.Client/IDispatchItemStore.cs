using WebHook.Contracts.Events;

namespace WebHook.DispatchItemStore.Client;

public readonly record struct DispatchItem(Guid Id, string EndpointUrl, IEvent Event);

public interface IDispatchItemStore
{
    void Put(DispatchItem item);

    void Remove(DispatchItem item);

    DispatchItem GetNext();

    void PersistChanges();
}