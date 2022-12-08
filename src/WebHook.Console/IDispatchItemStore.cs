using WebHook.Contracts.Events;

namespace WebHook.Console;

public readonly record struct DispatchItem(string EndpointUrl, IEvent Event);

public interface IDispatchItemStore
{
    void Put(DispatchItem item);

    void Remove(DispatchItem item);
    
    void PersistChanges();
}

public class DispatchItemStoreMock : IDispatchItemStore
{
    public void PersistChanges()
    {
    }

    public void Put(DispatchItem item)
    {
    }

    public void Remove(DispatchItem item)
    {
    }
}
