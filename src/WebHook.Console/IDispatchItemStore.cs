namespace WebHook.Console;

public readonly record struct DispatchItem(string EndpointUrl, IEvent Event);

public interface IDispatchItemStore
{
    void Put(DispatchItem item);

    void Remove(DispatchItem item);
    
    void PersistChanges();
}
